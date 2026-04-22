using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.Tenders;
using HtmlAgilityPack;
using System.Globalization;

namespace CleanAspireApp.Application.Services;

public class TenderService : ITenderService
{
    public enum AbbrMonth
    {
        Jan,
        Feb,
        Mar,
        Apr,
        May,
        Jun,
        Jul,
        Aug,
        Sep,
        Oct,
        Nov,
        Dec,
    }

    public async Task<List<EasyTender>> GetTendersAsync()
    {
        List<EasyTender> valuationTenders = new List<EasyTender>();
        var url = "https://easytenders.co.za/tenders?sort=&search=VALUATION+ROLL&province=all&company=&industry=any&status=all-tenders&filter=1#google_vignette";
        var client = CreateHttpClient(new Uri(url));
        var htmlDoc = new HtmlDocument();
        var html = await client.GetStringAsync(url);
        htmlDoc.LoadHtml(html);

        try
        {
            HtmlNodeCollection divMain = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tender-listing')]");
            if (divMain is not null && divMain.Count >= 1)
            {
                var tenderDivs = divMain[0].SelectNodes(".//div[contains(@class, 'card') and contains(@class, 'w-100') and contains(@class, 'mb-3') and contains(@class, 'tender')]");
                if (tenderDivs is not null)
                {
                    foreach (var div in tenderDivs)
                    {
                        string tenderUrl = string.Empty;
                        string province = string.Empty;
                        string imageSrc = string.Empty;
                        var innerHtml = div.InnerHtml;
                        var divHtml = new HtmlDocument();
                        divHtml.LoadHtml(innerHtml);
                        var anchorTags = divHtml.DocumentNode.SelectSingleNode("//a");
                        var anchorNode = divHtml.DocumentNode.SelectSingleNode("//a[@class='text-dark']");
                        var href = anchorNode?.GetAttributeValue("href", string.Empty);
                        tenderUrl = href?.ToString();


                        var clientLink = CreateHttpClient(new Uri(tenderUrl));
                        var htmlLinkDoc = new HtmlDocument();
                        var htmlLink = await client.GetStringAsync(tenderUrl);
                        htmlLinkDoc.LoadHtml(htmlLink);

                        HtmlNodeCollection cardBody = htmlLinkDoc.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]");
                        if (cardBody is not null)
                        {
                            HtmlNode imgNode = cardBody[1].SelectSingleNode("//img[contains(@class,'card-img border')]");
                            if (imgNode is not null)
                            {
                                imageSrc = imgNode.GetAttributeValue("src", string.Empty);
                            }

                            // Extract <link itemprop='thumbnailUrl'> href
                            //var linkNode = cardBody[1].SelectSingleNode("//link[@itemprop='thumbnailUrl']");
                            //string thumbUrl = linkNode.GetAttributeValue("href", null!);
                        }

                        var detailsDiv = htmlLinkDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tab-pane fade')]");
                        var tab1 = detailsDiv[0];
                        var tab2 = detailsDiv[1];
                        var tab3 = detailsDiv[2];

                        var downloadHtml = tab2.InnerHtml;
                        var downloadHtmlDoc = new HtmlDocument();
                        downloadHtmlDoc.LoadHtml(downloadHtml);
                        var downloadAnchorNode = downloadHtmlDoc.DocumentNode.SelectSingleNode("//a");
                        var downloadHref = downloadAnchorNode?.GetAttributeValue("href", string.Empty);

                        var paragraphs = tab1.SelectNodes(".//p");
                        int blankNdx = 0;
                        for (int i = 0; i < paragraphs.Count - 1; i++)
                        {
                            var par = paragraphs[i];
                            if (par.InnerText != string.Empty)
                            {
                                blankNdx = i;
                                break;
                            }

                        }

                        if (paragraphs.Count >= blankNdx + 4)
                        {

                            try
                            {
                                var detailsNode = paragraphs[blankNdx];
                                var datesNode = paragraphs[blankNdx + 1];
                                var contactsNode = paragraphs[blankNdx + 2];
                                var breifingNode = paragraphs[blankNdx + 3];
                                var conditionsNode = paragraphs[blankNdx + 4];
                                //details
                                var detailNodes = detailsNode.SelectNodes(".//strong");
                                var bidNumberNode = detailNodes[0];
                                string bidNumber = bidNumberNode.NextSibling.InnerText.Trim();
                                var departmentNode = detailNodes[1];
                                string department = departmentNode.NextSibling.InnerText.Trim();
                                var descriptionNode = detailNodes[2];
                                string description = descriptionNode.NextSibling.InnerText.Trim();
                                //dates
                                var dateLines = datesNode.SelectNodes(".//strong");
                                var openingDateNode = dateLines[0];
                                string openingDateLabel = openingDateNode.InnerText.Trim();
                                string openingDate = openingDateNode.NextSibling.InnerText.Trim();
                                //Debug.Print($"{openingDateLabel}{openingDate}");
                                var closingDateNode = dateLines[1];
                                string closingDateLabel = closingDateNode.InnerText.Trim();
                                string closingDate = closingDateNode.NextSibling.InnerText.Trim();

                                var closingDateDate = ConvertClosingDate(closingDate);
                                var openingDateDate = ConvertOpeningDate(openingDate);

                                bool tenderClosed = false;
                                double daysBetween;

                                if (closingDateDate > DateTime.Today)
                                {
                                    tenderClosed = false;
                                    daysBetween = (closingDateDate - DateTime.Today).Days;
                                }
                                else
                                {
                                    tenderClosed = true;
                                    daysBetween = (DateTime.Today - closingDateDate).Days;
                                }

                                //Debug.Print($"{closingDateLabel}{closingDate}");
                                var modifiedDateNode = dateLines[2];
                                //Contact
                                var contactNodes = contactsNode.SelectNodes(".//strong");
                                var contactLine = contactsNode.SelectSingleNode(".//strong");
                                string contact = contactLine.NextSibling.InnerText.Substring(contactLine.NextSibling.InnerText.IndexOf(" ")).Trim();
                                string[] contactText = contactsNode.InnerText.Split(":");
                                string contactNumber = contactText[3];


                                EasyTender vallRollTender = new EasyTender
                                {
                                    TenderNumber = bidNumber,
                                    OpeningDate = DateOnly.FromDateTime(openingDateDate),
                                    ClosingDate = DateOnly.FromDateTime(closingDateDate),
                                    TenderUrl = tenderUrl,
                                    TenderDocument = downloadHref,
                                    Description = description,
                                    Expired = tenderClosed,
                                    Age = (int)daysBetween,
                                    MunicipalityName = department,
                                    Logurl = imageSrc,
                                };
                                valuationTenders.Add(vallRollTender);
                            }
                            catch (Exception)
                            {

                                throw;
                            }


                        }


                    }
                }
                return valuationTenders;
            }
        }
        catch (OperationCanceledException ex)
        {

            Console.WriteLine($"Operation cancelled{ex.Message}");
        }

        return new List<EasyTender>();
    }

    //private Task<TenderDetails> ExtractDeatilsFromDiv(HtmlNode div)
    //{
    //    throw new NotImplementedException();
    //}

    public DateTime ConvertClosingDate(string dt)
    {
        string newDt = dt.Substring(dt.IndexOf(",") + 1).Trim();
        string[] splitDt = newDt.Split(" ");
        int monthNumber = GetMonthPosition(splitDt[1]);
        var actual = DateTimeFormatInfo.CurrentInfo.GetMonthName(monthNumber + 1);

        string dateForFormat = $"{dt.Substring(0, dt.IndexOf(",")).Trim()}, {splitDt[0]} {actual} {splitDt[2]} {splitDt[3]}";

        DateTime dateTime = DateTime.ParseExact(dateForFormat, "dddd, d MMMM yyyy h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
        return dateTime;
    }

    public DateTime ConvertOpeningDate(string dt)
    {
        string newDt = dt.Substring(dt.IndexOf(",") + 1).Trim();
        string[] splitDt = newDt.Split(" ");
        int monthNumber = GetMonthPosition(splitDt[1]);
        var actual = DateTimeFormatInfo.CurrentInfo.GetMonthName(monthNumber + 1);

        string dateForFormat = $"{dt.Substring(0, dt.IndexOf(",")).Trim()}, {splitDt[0]} {actual} {splitDt[2]}";

        DateTime dateTime = DateTime.ParseExact(dateForFormat, "dddd, d MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
        return dateTime;
    }

    public int GetMonthPosition(string monthName)
    {
        if (Enum.TryParse(typeof(AbbrMonth), monthName, out var monthEnum))
        {
            return (int)monthEnum;
        }
        throw new ArgumentException("Invalid month name");
    }

    private static HttpClient CreateHttpClient(Uri baseAddress)
    {
        var client = new HttpClient()
        {
            BaseAddress = baseAddress,
        };
        return client;
    }


    private class TenderDetails
    {
        public string TenderNumber { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;

    }

}


