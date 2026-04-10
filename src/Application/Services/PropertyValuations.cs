using CleanAspireApp.Application.Interfaces;
using CleanAspireApp.Domain.ValuationRoll;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace CleanAspireApp.Application.Services;

public class PropertyValuations : IPropertyValuation
{
    private readonly string _baseUrlErf = "https://web1.capetown.gov.za/web1/gv2025/Results?Search=ERF,";

    public async Task<List<PropertyRecord>> GetAllValuations(string erf)
    {
        var url = $"{_baseUrlErf}{erf}";
        List<PropertyRecord> parcels = await GetAllParcelsAsync(url);

        return parcels;
    }

    private async Task<List<PropertyRecord>> GetAllParcelsAsync(string url)
    {
        var web = new HtmlWeb();
        var doc = web.Load(url);
        List<PropertyRecord> propertyRecords = await ExtractRows(doc);
        foreach (var record in propertyRecords)
        {
            record.PageNumber = "1";
        }
        List<PropertyRecord> allPages = await Paginator(url);
        propertyRecords.AddRange(allPages);
        return propertyRecords;
    }

    private static async Task<List<PropertyRecord>> ExtractRows(HtmlDocument doc)
    {
        {
            string baseUrlPropertyReference = "https://web1.capetown.gov.za/web1/gv2025/Results?Search=VAL,";
            List<PropertyRecord> propertyRecords = new List<PropertyRecord>();
            var rows = doc.DocumentNode.SelectNodes("//table//tr");
            if (rows != null)
            {
                for (int i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var cells = row.SelectNodes("td");
                    if (cells != null && cells.Count >= 10)
                    {
                        string erf = cells[1].InnerText.Trim();

                        var firstSpace = erf.IndexOf(' ');
                        string erfNumberPart = string.Empty;
                        string allotmentPart = string.Empty;
                        string stringMarketValue = cells[5].InnerText.Trim();
                        string cleanedMarketValue = ParseRandValueSafe(stringMarketValue).ToString();
                        if (firstSpace > 0)
                        {
                            erfNumberPart = erf[..firstSpace];
                            allotmentPart = erf[(firstSpace + 1)..];
                        }
                        var record = new PropertyRecord
                        {
                            PropertyReference = cells[0].InnerText.Trim(),
                            ErfNumber = erf,
                            RatingCategory = cells[2].InnerText.Trim(),
                            Address = cells[3].InnerText.Trim(),
                            Extent = double.TryParse(cells[4].InnerText.Trim(), out var extent) ? extent : 0,
                            MarketValue = cleanedMarketValue,
                            EffectiveDate = cells[9].InnerText.Trim(),
                            DisputeExpiryDate = cells.Count > 10 ? cells[10].InnerText.Trim() : string.Empty,
                            Allotment = allotmentPart,
                            Erf = erfNumberPart,
                            Description = cells[1].InnerText.Trim(),
                            Link = $"{baseUrlPropertyReference}{cells[0].InnerText.Trim()}"
                        };
                        propertyRecords.Add(record);
                    }
                }
                return propertyRecords;
            }
            return new List<PropertyRecord>();
        }
    }

    private static async Task<List<PropertyRecord>> Paginator(string url)
    {
        List<PropertyRecord> allPropertyRecords = new List<PropertyRecord>();
        var web = new HtmlWeb();
        var doc = web.Load(url);
        string viewState = doc.DocumentNode.SelectSingleNode("//input[@name='__VIEWSTATE']")?.GetAttributeValue("value", "") ?? "";
        string eventValidation = doc.DocumentNode.SelectSingleNode("//input[@name='__EVENTVALIDATION']")?.GetAttributeValue("value", "") ?? "";
        string viewStateGenerator = doc.DocumentNode.SelectSingleNode("//input[@name='__VIEWSTATEGENERATOR']")?.GetAttributeValue("value", "") ?? "";

        var links = doc.DocumentNode.SelectNodes("//tr[@class='DataGridPager']//a");

        if (links is not null)
        {
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", ""); //<a href="javascript:__doPostBack(&#39;dgSearch$ctl14$ctl02&#39;,&#39;&#39;)">3</a>
                href = WebUtility.HtmlDecode(href); //javascript:__doPostBack('dgSearch$ctl14$ctl02','')

                var match = Regex.Match(href, @"__doPostBack\('([^']*)','([^']*)'\)");

                if (match.Success)
                {
                    var eventTarget = match.Groups[1].Value;
                    var pageNumber = link.InnerText.Trim();
                    var postData = new Dictionary<string, string>
                                         {
                                             {"__VIEWSTATE", viewState },
                                             {"__EVENTVALIDATION", eventValidation },
                                             {"__VIEWSTATEGENERATOR", viewStateGenerator },
                                             {"__EVENTTARGET", eventTarget },
                                             {"__EVENTARGUMENT", "" }
                                         };

                    var client = new HttpClient();

                    var content = new FormUrlEncodedContent(postData);
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Process the response for the current page
                    var pageDoc = new HtmlDocument();
                    pageDoc.LoadHtml(responseString);
                    var propertyRecords = await GetPageParcelsAsync(pageDoc, pageNumber);
                    allPropertyRecords.AddRange(propertyRecords);

                }
            }
            return allPropertyRecords;
        }


        return new List<PropertyRecord>();
    }

    private static async Task<IEnumerable<PropertyRecord>> GetPageParcelsAsync(HtmlDocument pageDoc, string pageNumber)
    {
        List<PropertyRecord> propertyRecords = await ExtractRows(pageDoc);
        foreach (var record in propertyRecords)
        {
            record.PageNumber = pageNumber;
        }
        propertyRecords.RemoveAt(0);
        return propertyRecords;
    }

    private static async Task<List<Sales>> GetPropertySalesAsync(string propertyReference)
    {
        string baseUrlSales = "https://web1.capetown.gov.za/web1/gv2025/Sales?parcelid=";
        List<Sales> salesRecords = new List<Sales>();
        var url = $"{baseUrlSales}{propertyReference}";
        var web = new HtmlWeb();
        var doc = web.Load(url);

        //There are a few tables. The Sales are in Table 4

        var tables = doc.DocumentNode.SelectNodes("//table");
        var salesTable = tables[3]; // index 3 = 4th table

        var rows = salesTable.SelectNodes(".//tr");
        if (rows != null)
        {
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var cells = row.SelectNodes("td");
                if (cells != null && cells.Count >= 7)
                {
                    var record = new Sales
                    {
                        PropertyReference = cells[0].InnerText.Trim(),
                        Address = cells[1].InnerText.Trim(),
                        Description = cells[2].InnerText.Trim(),
                        ErfExtent = cells[3].InnerText.Trim(),
                        DwellingExtent = cells[4].InnerText.Trim(),
                        SaleDate = cells[5].InnerText.Trim(),
                        SalePrice = cells[6].InnerText.Trim()
                    };
                    salesRecords.Add(record);
                }
            }
            return salesRecords;
        }
        return new List<Sales>();
    }

    public async Task<PropertyRecord> GetPropertyValuation(string erf, string allotment)
    {
        string encodedAllotment = WebUtility.UrlEncode(allotment);
        string geocoderUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";
        var parcels = await GetAllValuations(erf);
        var thisProperty = parcels.Where(p => p.Erf == erf && allotment.Contains(p.Allotment)).FirstOrDefault();
        if (thisProperty != null)
        {
            var salesRecords = await GetPropertySalesAsync(thisProperty.PropertyReference);
            if (thisProperty != null)
            {
                foreach (var sale in salesRecords)
                {
                    var url = geocoderUrl + $"?f=json&singleLine={Uri.EscapeDataString(sale.Address)}";
                    sale.AddressLocator = url;
                }
                thisProperty.SalesRecords = salesRecords;
                return thisProperty;
            }
        }
        return null!;
    }

    public static decimal ParseRandValueSafe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var cleaned = value
            .Replace("R", "")
            .Replace(" ", "")
            .Replace(",", "");

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }

    public async Task<List<PropertyRecord>> GetAllSSValuations(string schemeName)
    {
        string encodedSchemeName = Uri.EscapeDataString(schemeName);
        string geocoderUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";
        string baseUrlSS = "https://web1.capetown.gov.za/web1/gv2025/ResSecTit?Search=SE1,";
        var url = $"{baseUrlSS}{encodedSchemeName}";

        var web = new HtmlWeb();
        var doc = web.Load(url);
        List<PropertyRecord> propertyRecords = await ExtractRows(doc);
        foreach (var record in propertyRecords)
        {
            record.PageNumber = "1";
            var salesRecords = await GetPropertySalesAsync(record.PropertyReference);
            foreach (var sale in salesRecords)
            {
                var addressUrl = geocoderUrl + $"?f=json&singleLine={Uri.EscapeDataString(sale.Address)}";
                sale.AddressLocator = addressUrl;
            }
            record.SalesRecords = salesRecords;

        }
        List<PropertyRecord> allPages = await Paginator(url);
        propertyRecords.AddRange(allPages);
        return propertyRecords;

    }

    public async Task<PropertyRecord> GetAllFarmValuations(string farm)
    {
        string baseUrlFarm = "https://web1.capetown.gov.za/web1/gv2025/Results?Search=FA2,";
        string baseUrlPropertyReference = "https://web1.capetown.gov.za/web1/gv2025/Results?Search=VAL,";
        string geocoderUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";

        List<PropertyRecord> propertyRecords = new List<PropertyRecord>();
        string[] farmDesc = farm.Split(',');
        var url = $"{baseUrlFarm}{farmDesc[0]},{farmDesc[1]}";
        var web = new HtmlWeb();
        var doc = web.Load(url);

        var rows = doc.DocumentNode.SelectNodes("//table//tr");
        if (rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var hangHoldRecords = new List<PropertyRecord>();
                decimal maxMarketValue = 0;
                var row = rows[i];
                var cells = row.SelectNodes("td");
                string propertyDescription;
                if (cells != null && cells.Count >= 10)
                {
                    string erf = cells[1].InnerText.Trim();
                    //string coctDescription = $"{farmDesc[2]} FARMS {farmDesc[0]} - {farmDesc[1]}";
                    propertyDescription = cells[1].InnerText.Trim();
                    string stringMarketValue = cells[5].InnerText.Trim();
                    string cleanedMarketValue = ParseRandValueSafe(stringMarketValue).ToString();
                    var record = new PropertyRecord
                    {
                        PropertyReference = cells[0].InnerText.Trim(),
                        ErfNumber = erf,
                        RatingCategory = cells[2].InnerText.Trim(),
                        Address = cells[3].InnerText.Trim(),
                        Extent = double.TryParse(cells[4].InnerText.Trim(), out var extent) ? extent : 0,
                        MarketValue = cleanedMarketValue,
                        EffectiveDate = cells[9].InnerText.Trim(),
                        DisputeExpiryDate = cells.Count > 10 ? cells[10].InnerText.Trim() : string.Empty,
                        Erf = farm,
                        Description = cells[1].InnerText.Trim(),
                        Allotment = $"{farmDesc[2]} FARMS",
                        Link = $"{baseUrlPropertyReference}{cells[0].InnerText.Trim()}"
                    };

                    if (propertyDescription.Contains("VALUED WITH :"))
                    {
                        string[] holdingProperty = propertyDescription.Split(":");
                        string holdingPropertyReference = holdingProperty[1].Trim();
                        //now use this url to get the hanging property details.
                        var holdingPropertyUrl = $"{baseUrlPropertyReference}{holdingPropertyReference}";
                        record.HoldingLink = holdingPropertyUrl;
                        var web1 = new HtmlWeb();
                        var docHanging = web1.Load(holdingPropertyUrl);
                        var rowsHanging = docHanging.DocumentNode.SelectNodes("//table//tr");

                        if (rowsHanging != null)
                        {
                            for (int j = 1; j < rowsHanging.Count; j++)
                            {
                                var rowHanging = rowsHanging[j];
                                var cellsHanging = rowHanging.SelectNodes("td");

                                if (cellsHanging != null && cellsHanging.Count >= 10)
                                {
                                    string stringMarketValue1 = cellsHanging[5].InnerText.Trim();
                                    decimal marketValue = ParseRandValueSafe(stringMarketValue1);
                                    if (marketValue > maxMarketValue)
                                    {
                                        maxMarketValue = marketValue;
                                    }
                                    var recordHanging = new PropertyRecord
                                    {
                                        PropertyReference = cellsHanging[0].InnerText.Trim(),
                                        Address = cellsHanging[3].InnerText.Trim(),
                                        MarketValue = stringMarketValue1,
                                        EffectiveDate = cellsHanging[9].InnerText.Trim(),
                                        DisputeExpiryDate = cellsHanging.Count > 10 ? cellsHanging[10].InnerText.Trim() : string.Empty,
                                        Description = cellsHanging[1].InnerText.Trim(),
                                    };
                                    hangHoldRecords.Add(recordHanging);
                                }

                            }


                        }
                    }

                    if (hangHoldRecords.Count > 1)
                    {
                        hangHoldRecords.RemoveAt(0);
                    }
                    if (maxMarketValue > 0)
                    {

                        record.MarketValue = maxMarketValue.ToString();
                    }
                    record.ValuedTogether = hangHoldRecords;
                    propertyRecords.Add(record);
                }
            }
            if (propertyRecords.Count > 0)
            {
                propertyRecords.RemoveAt(0);
                var thisFarm = propertyRecords.LastOrDefault();
                if (thisFarm != null)
                {
                    var salesRecords = await GetPropertySalesAsync(thisFarm.PropertyReference);

                    foreach (var sale in salesRecords)
                    {
                        var farmUrl = geocoderUrl + $"?f=json&singleLine={Uri.EscapeDataString(sale.Address)}";
                        sale.AddressLocator = farmUrl;
                    }
                    thisFarm.SalesRecords = salesRecords;
                    return thisFarm;
                }
            }
        }
        return new PropertyRecord();
    }


    /// <summary>
    /// A List can be returned. I must handle the correct one from the GIS.
    /// </summary>
    /// <param name="schemeName"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public async Task<List<PropertyRecord>> GetAllSSUnitValuations(string schemeName, string unit)
    {

        string encodedSchemeName = Uri.EscapeDataString(schemeName);
        string baseUrlSS = "https://web1.capetown.gov.za/web1/gv2025/ResSecTit?Search=SE2,";
        var url = $"{baseUrlSS}{encodedSchemeName},{unit}";

        var web = new HtmlWeb();
        var doc = web.Load(url);
        List<PropertyRecord> propertyRecords = await ExtractRows(doc);
        foreach (var record in propertyRecords)
        {
            record.PageNumber = "1";
        }
        List<PropertyRecord> allPages = await Paginator(url);
        propertyRecords.AddRange(allPages);
        return propertyRecords;


    }
}


