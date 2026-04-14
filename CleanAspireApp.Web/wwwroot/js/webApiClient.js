const [GraphicsLayer, Graphic, CentroidOperator, webMercatorUtils, Map, TextSymbol] = await $arcgis.import([
    "@arcgis/core/layers/GraphicsLayer.js",
    "@arcgis/core/Graphic.js",
    "@arcgis/core/geometry/operators/centroidOperator.js",
    "@arcgis/core/geometry/support/webMercatorUtils",
    "@arcgis/core/Map.js",
    "@arcgis/core/symbols/TextSymbol.js",
]);
import { findMunicipality } from "./mapLoader.js";
import { saveMapState } from "./main.js";
import { saveGraphics } from "./main.js";

import { logToDotNet } from "./DotnetLogger.js";
import { downloadSales, formatRand } from "./fileUtils.js";


let theseSales = null;
let theseUnits = null;


if (!window.__config) {
    await loadConfig().then(() => {
        if (!window.__config)
        {
            console.log("Config failed to load");   
            return;
        }
    });
} 



export async function getSSValuation(feature, view, map)
{
    if (!window.__config) {
        await loadConfig();
    } 
    const schemeName = feature.attributes.ST_SCHM_NAME;
    const schemeNumber = feature.attributes.ST_SCHM_NO;
    const schemeYear = feature.attributes.ST_SCHM_YEAR;
    const centroid = CentroidOperator.execute(feature.geometry);
    const wgs84Point = webMercatorUtils.webMercatorToGeographic(centroid);
    const endpoint = window.__config.apiBaseUrl + "/api/valuations/SchemeValuation/" + schemeName.replace(" ", "%20");
    try {
        localStorage.setItem("endpoint", endpoint);
        localStorage.setItem("propertyLocation", JSON.stringify(wgs84Point));
        document.querySelector("calcite-loader").hidden = false;
        const response = await fetch(`${endpoint}`, {
            method: 'GET'
        });
        if (response.ok)
        {
            document.querySelector("calcite-loader").hidden = true;
            const rollData = await response.json();
            if (rollData )
            {
                 createSSGraphicPoint(view, wgs84Point.x, wgs84Point.y, schemeName);

                if (schemeNumber === null)
                {
                    /*Will assume that there are no other schemes with the same scheme name.*/
                    const firstUnit = rollData[0];
                    localStorage.setItem("lastValuation", JSON.stringify(firstUnit));
                    console.log("No Scheme number but cannot isolate to this scheme. Returning all units");
                    addUnits(rollData)
                    return rollData;
                } else {
                    //console.log("Current dataset", rollData.length);
                    const trimmedSchemeNumber = schemeNumber.replace("SS","");
                    ////get the first unit to see if it can be split. Will only work with SS+the scheme number
                    const canSplit = 1;
                    const unit = rollData[1];
                    const allot = unit.Allotment;
                    
                        const allotSplit = allot.split(" SS")[1] ?? null;
                        
                        if (allotSplit === null)
                        {
                            const firstUnit = rollData[0];
                            localStorage.setItem("lastValuation", JSON.stringify(firstUnit));
                            console.log("Has a Scheme number but cannot isolate to this scheme. Returning all units");
                            addUnits(rollData)
                            return rollData;
                            
                        } else {
                            const filtered = rollData.filter((unit, i) => {
                                //if (i === 0) return true; // keep header row if needed
                                const allot = unit.Allotment;
                                const parts = allot.split(" SS");
                                if (parts.length < 2) return false;

                                const splitted = parts[1];
                                const _year = splitted.split("/")[1]?.split("-")[0] ?? null;
                                const _number = splitted.split("/")[0] ?? null;

                                return trimmedSchemeNumber === _number && schemeYear === _year;
                            });
                            //Sometimes the SG or the CoCT may have the incorrect scheme number.
                            //I may need to get the street address from the first unit and then find the scheme number based on the street address and then filter again based on the scheme number from the street address.
                            //the only way I can do that is to reverse geocode the point.
                            if (filtered.length === 0)
                            {
                                const firstUnit = rollData[0];
                                localStorage.setItem("lastValuation", JSON.stringify(firstUnit));
                                console.log("Has a Scheme number but could not filter on scheme number. Their is an inconsistency with the SG and CoCT scheme number");
                                addUnits(rollData)
                                return rollData;
                            }

                            const firstUnit = rollData[0];
                            localStorage.setItem("lastValuation", JSON.stringify(firstUnit));
                            console.log("Has a Scheme number and limited units to this Scheme only");
                            addUnits(filtered)
                            return filtered;
                        }
                }
            }
        }
        else
        {
            document.querySelector("calcite-loader").hidden = true;
        }
    } catch (error) {

        document.querySelector("calcite-loader").hidden = true;
    }
}

export async function getFarmMarketValue(feature, view,map)
{
    if (!window.__config) {
        await loadConfig();
    } 
    const farmNumber = feature.attributes.PARCEL_NO;
    const portion = feature.attributes.PORTION;
    const region = feature.attributes.MAJ_REGION;

    const centroid = CentroidOperator.execute(feature.geometry);
    const wgs84Point = webMercatorUtils.webMercatorToGeographic(centroid);

    const parameterString = `${farmNumber},${portion},${region}`;
    const endpoint = window.__config.apiBaseUrl + "/api/valuations/FarmValuation/" + parameterString;

    try {
        document.querySelector("calcite-loader").hidden = false;
        const response = await fetch(`${endpoint}`, {
            method: 'GET'
        });
        
        if (!response.ok) {
            throw new Error(data.message || 'Error fetching valuation roll');
        } else {
            const data = await response.json();
            if (data)
            {
                localStorage.setItem("endpoint", endpoint);
                localStorage.setItem("propertyLocation", JSON.stringify(wgs84Point));
                localStorage.setItem("lastValuation", JSON.stringify(data));
                var sales = data.Sales;
                document.querySelector("calcite-loader").hidden = true;
                addSalesRecords(sales);
                createGraphicPoint(view, wgs84Point.x, wgs84Point.y, data);
                saveMapState(view);
                return data;
            } else {
                showStatus("Nothing returned from the CoCT website...");
                document.querySelector("calcite-loader").hidden = true;
            }
        }
        
    }
    catch (error) {
        console.error('Error fetching valuation roll data:', error);
        document.querySelector("calcite-loader").hidden = true;
    }

}

export async function getMarketValue(feature, view,map) {
    // Ensure config is loaded before using it
    if (!window.__config) {
        await loadConfig();
    } 
    const erfName = feature.attributes.TAG_VALUE.replace("RE/ ", "");
    const minRegion = feature.attributes.MIN_REGION;

    const centroid = CentroidOperator.execute(feature.geometry);

    const wgs84Point = webMercatorUtils.webMercatorToGeographic(centroid);

    const endpoint = window.__config.apiBaseUrl + "/api/valuations/ErfValuation/" + erfName.replace("RE/", "") + "/" + minRegion.replace(" ", "%20"); //
    //console.log("Endpoint:", endpoint);
    try
    {
        document.querySelector("calcite-loader").hidden = false;
        const response = await fetch(`${endpoint}`, {
            method: 'GET'
        });
        if (!response.ok) {
            throw new Error(data.message || 'Error fetching valuation roll');
            showStatus("Error Getting the property data from the CoCT website...");
        } else
        {
            const data = await response.json();
            if (data) {
                localStorage.setItem("endpoint", endpoint);
                localStorage.setItem("lastValuation", JSON.stringify(data));
                localStorage.setItem("propertyLocation", JSON.stringify(wgs84Point));
                document.querySelector("calcite-loader").hidden = true;
                const sales = data.Sales;
                theseSales = sales;
                addSalesRecords(sales);
                createGraphicPoint(view, wgs84Point.x, wgs84Point.y, data);
                saveMapState(view);
                return data;
            } else
            {
                showStatus("Nothing returned from the CoCT website...");
            }
        }
        
    } catch (error)
    {
        console.error('Error fetching valuation roll data:', error);
        document.querySelector("calcite-loader").hidden = true;

    }
}

function showStatus(msg) {
    document.getElementById("status").innerText = msg;
}


function addUnits(units) {
    //I may want to just write the first unit to local storage and then get the rest of the units from the first unit. This is because the first unit will have the same scheme number as the rest of the units and I can use that to filter the units on the client side if needed.
    //there seems to be an issue when loading the local storage when it is too large.
    const tbodyUnits = document.getElementById("ssUnitsTable");
    if (!tbodyUnits) {
        console.warn("Table not ready yet");
        return;
    }

    while (tbodyUnits.firstChild) {
        tbodyUnits.removeChild(tbodyUnits.firstChild);
    }

    const length = units.length;

    //<div id="button">
    //    <button id="download-units-btn" class="zoom-btn" >
    //        Download Units
    //    </button>
    //</div

    tbodyUnits.innerHTML = `
    <div>
           <table id="ssUnitsTable" class="address-table">
                <thead>
                    <tr>
                        <th>Property Reference</th>
                        <th>Address</th>
                        <th>Description</th>
                        <th>Rating Category</th>
                        <th>Extent</th>
                        <th>Market Value</th>
                        <th>Link</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
      </div>
    `;
    for (let i = 0; i < length; i++) {
        const propertyReference = units[i].PropertyReference;
        const address = units[i].Address;
        const description = units[i].Description;
        const ratingCat = units[i].RatingCategory;
        const extent = units[i].Extent;
        const marketValue = units[i].MarketValue;
        const link = units[i].Link;
        const row = document.createElement("tr");
        row.dataset.index = i;
        row.innerHTML = `
                            <td>${propertyReference}</td>
                            <td>${address}</td>
                            <td>${description}</td>
                            <td>${ratingCat}</td>
                            <td>${extent}</td>
                            <td>${marketValue}</td>
                            <td>
                               <a href="${link}" target="_blank" rel="noopener noreferrer" style="color: white; text-decoration: underline;">
                                     ${propertyReference}
                               </a>
                            </td>
                        `;

        //<td>
        //    <button class="coct-link-btn zoom-btn" data-address="${link}">
        //     CoCT Website
        //    </button>
        //</td>
        tbodyUnits.appendChild(row);
    }
    tbodyUnits.addEventListener("click", async (event) => {
        const btn = event.target.closest(".coct-link-btn");
        if (!btn) return;

        console.log("CoCT link button clicked", btn.dataset.address);

        const row = btn.closest("tr");
    });


    const btnDownloadUnits = document.getElementById("download-units-btn");
    btnDownloadUnits.addEventListener("click", () => {
        console.log("CoCT link download units clicked");
        /* downloadUnits(theseUnits);*/
    });

}

function addSalesRecords(sales)
{
    const tbody = document.getElementById("myTableContainer");

    if (!tbody) {
        console.warn("Table not ready yet");
        return;
    }

    while (tbody.firstChild) {
        tbody.removeChild(tbody.firstChild);
    }

    
    const length = sales.length;

    tbody.innerHTML = `
        <div id="button">
            <button id="download-btn" class="zoom-btn" >
                                Download Sales
                            </button>
        </div
        <div>
            <table id="addressTable" class="address-table">
                <thead>
                    <tr>
                        <th>Property Reference</th>
                        <th>Address</th>
                        <th>Description</th>
                        <th>Erf Extent</th>
                        <th>Building Extent</th>
                        <th>Sale Date</th>
                        <th>Sale Price</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    `;
    for (let i = 0; i < length; i++) {
        const addressLocatorUrl = sales[i].AddressLocator;
        const propertyReference = sales[i].PropertyReference;
        const address = sales[i].Address;
        const description = sales[i].Description;
        const erfExtent = sales[i].ErfExtent;
        const dwellingExtent = sales[i].DwellingExtent;
        const saleDate = sales[i].SaleDate;
        const salePrice = sales[i].SalePrice;
        const row = document.createElement("tr");
        row.dataset.index = i;
        row.innerHTML = `
                            <td>${propertyReference}</td>
                            <td>${address}</td>
                            <td>${description}</td>
                            <td>${erfExtent}</td>
                            <td>${dwellingExtent}</td>
                            <td>${salePrice}</td>
                            <td>${saleDate}</td>
                            <td style="display:none;">${addressLocatorUrl}</td>
                            <td>
                                <button id="zoom-btn" class="zoom-btn" data-address="${addressLocatorUrl}">
                                    Add To Map
                                </button>
                           </td>
                        `;
        tbody.appendChild(row);
    }

    tbody.addEventListener("click", async (event) => {

        const row = event.target.closest("tr");
        if (!row) return;
        const propertyRef = row.children[0].innerText;
        const address = row.children[7].innerText;
        const btn = event.target.closest(".zoom-btn");
        if (!btn) return; // ignore clicks on the row
        const addressLocatorUrl = btn.dataset.address;
        const index = Number(row.dataset.index);
        const currentSale = sales[index];
        await GeocodeAddress(addressLocatorUrl, currentSale);
    });
    const btnDownloadSales = document.getElementById("download-btn");
    btnDownloadSales.addEventListener("click", () => {
        downloadSales(theseSales);
    });

}

async function GeocodeAddress(addressLocatorUrl, currentSale)
{
    try {
        showStatus("Geocoding the sale...");
        const response = await fetch(`${addressLocatorUrl}`, {
            method: 'GET'
        });
        showStatus("Done geocoding the sale...");
        if (response.ok) {
            showStatus("Found sale from address...");
            const data = await response.json();
            const candidates = data.candidates || [];

            if (candidates.length > 0) {
                const firstCandidate = candidates[0];
                const location = firstCandidate.location;
                const lon = location.x;
                const lat = location.y;
                const point = {
                    type: "point",
                    longitude: lon,
                    latitude: lat
                };

               createSaleGraphicPoint(view, lon,lat, currentSale);
            } else {
                showStatus("Could not geocode address...");
            }
        }

    } catch (error) {
        console.log("Error geocoding address:", error);
    }
   

}

async function getSales(data) 
{
    /*const geocodingService = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";*/

    showStatus("Geocoding the sales...");

    const sales = data.salesRecords;
    const length = sales.length;
    //console.log(`Sales count: ${length}`);
    for (let i = 0; i < length; i++)
    {
        let addressLocatorUrl = sales[i].addressLocator;
        const response = await fetch(`${addressLocatorUrl}`, {
            method: 'GET'
        });

        if (response.ok)
        {
            const data = await response.json();
            const candidates = data.candidates || [];

            if (candidates.length > 0) {
                const firstCandidate = candidates[0];
                const location = firstCandidate.location;
                const lon = location.x;
                const lat = location.y;
               /* console.log(`Geocoded location: lat ${lat}, lon ${lon}`);*/
                //console.log("First candidate:", firstCandidate);
            } else {
                console.log("No candidates found");
            }
        }
    }
    showStatus("Done!Geocoding the sales...");

}

function zoomToFeature(view, geometry) {
    view.goTo({
        target: geometry,
        zoom: 21 // Adjust zoom level as needed
    }).catch(function (error) {
        console.error("Error zooming to feature:", error);
    });
}

async function createSaleGraphicPoint(view, lon,lat, sale)
{
    // Create a point geometry
    const point = {
        type: "point",
        longitude: lon,
        latitude: lat,
    };

    // Create a simple marker symbol
    const markerSymbol = {
        type: "simple-marker",
        color: "red",
        size: 10
    };

    // Create a picture marker symbol (custom icon)
    const markerSymbolPicture = {
        type: "picture-marker",  // Use picture-marker for custom icons
        url: "https://developers.arcgis.com/javascript/latest/sample-code/sandbox/images/blue-pin.png", // Icon URL 
        width: "24px",
        height: "24px"
    };

    // Create marker graphic
    const markerGraphic = new Graphic({
        geometry: point,
        symbol: markerSymbol
    });

    // Create a graphic
    const pointGraphic = new Graphic({
        geometry: point,
        symbol: markerSymbol,
        attributes: {
            salePrice: sale.SalePrice,
            saleDate: sale.SaleDate,
            address: sale.Address,
            erfExtent: sale.ErfExtent,
            dwellingExtent: sale.DwellingExtent,
            description: sale.Description,
            
        },
        popupTemplate: {
            title: "Sale",
            content: "Sale Price: {salePrice} </br></br>  Sale Date: {saleDate} </br></br> Dwelling Extent: {dwellingExtent} </br></br> Erf Extent: {erfExtent} ",
        },
    });
    // Get all GraphicsLayers from the map
    const graphicsLayers = map.layers.filter(layer => layer.type === "graphics");
    // Sales graphics layer is the second one.
    const salesGraphicsLayer = graphicsLayers.getItemAt(1);
    // Add graphic to the map's view
    salesGraphicsLayer.graphics.add(pointGraphic);
    showStatus("Added sale to layer...");


    //const zoomPoint = {
    //    type: "point",
    //    longitude: lon,
    //    latitude: lat,
    //};
    //zoomToFeature(view, zoomPoint);
}

async function createSSGraphicPoint(view, lon, lat, scheme) {

    
    //console.log("data from graphic point", data);
    // Create a point geometry
    const point = {
        type: "point",
        longitude: lon,
        latitude: lat
    };

    // Create a simple marker symbol
    const markerSymbol = {
        type: "simple-marker",
        color: "purple",
        size: 10
    };


    // Label as a TextSymbol
    const textSymbol = new TextSymbol({
        text: scheme,
        color: "yellow",
        font: {
            family: "Josefin Sans",
            style: "normal",
            weight: "bold",
            size: 10,
        },
        yoffset: 10 // Move label above the point
    });
    //haloColor: "blue",
    //    haloSize: 1,

    // Create marker graphic
    const markerGraphic = new Graphic({
        geometry: point,
        symbol: markerSymbol
    });


    // Create a graphic
    const pointGraphic = new Graphic({
        geometry: point,
        symbol: textSymbol,
        attributes: {
            schemeName: async function createGraphicPoint(view, lon,lat,data)
{

    const formattedValue = formatRand(data.MarketValue);
    //console.log("data from graphic point", data);
    // Create a point geometry
     const point = {
        type: "point",
        longitude: lon,
        latitude: lat
    };

    // Create a simple marker symbol
    const markerSymbol = {
        type: "simple-marker",
        color: "purple",
        size: 10
    };


    // Label as a TextSymbol
    const textSymbol = new TextSymbol({
        text: formattedValue,
        color: "yellow",
        font: {
            family: "Josefin Sans",
            style: "normal",
            weight: "bold",
            size: 10,
        },
        yoffset: 10 // Move label above the point
    });
    //haloColor: "blue",
    //    haloSize: 1,

    // Create marker graphic
    const markerGraphic = new Graphic({
        geometry: point,
        symbol: markerSymbol
    });


    // Create a graphic
    const pointGraphic = new Graphic({
        geometry: point,
        symbol: textSymbol,
        attributes: {
            schemeName: scheme,
            
        },
        popupTemplate: {
            title: "{ST_SCHM_NO}",
            content: "Scheme Number: {ST_SCHM_NO}",
        },
    });

   

    // Get all GraphicsLayers from the map
    const graphicsLayers = map.layers.filter(layer => layer.type === "graphics");
    // Get the first one (if any exist)
    const valuesGraphicsLayer = graphicsLayers.getItemAt(0);
    // Add graphic to the map's view
    valuesGraphicsLayer.graphics.addMany([markerGraphic,pointGraphic]);
},

        },
        popupTemplate: {
            title: "{description}",
            content: "Market Value: {formattedValue}</br></br/> Rating Category: {ratingCategory}",
        },
    });



    // Get all GraphicsLayers from the map
    const graphicsLayers = map.layers.filter(layer => layer.type === "graphics");
    // Get the first one (if any exist)
    const valuesGraphicsLayer = graphicsLayers.getItemAt(0);
    // Add graphic to the map's view
    valuesGraphicsLayer.graphics.addMany([markerGraphic, pointGraphic]);
}

async function createGraphicPoint(view, lon, lat, data)
{
    const formattedValue = formatRand(data.MarketValue);
    //console.log("data from graphic point", data);
    // Create a point geometry
     const point = {
        type: "point",
        longitude: lon,
        latitude: lat
    };

    // Create a simple marker symbol
    const markerSymbol = {
        type: "simple-marker",
        color: "purple",
        size: 10
    };


    const textSymbolMarketValue = {
        text: formattedValue,
        type: "text",
        color: "yellow",

        haloColor: "black",
        haloSize: 1.5,

       /* backgroundColor: [0, 0, 0, 0.75], // black semi-transparent background*/
        borderLineColor: [255, 255, 255, 0], // optional invisible border
        borderLineSize: 0,

        font: {
            size: 10,
            family: "Arial",
            weight: "bold"
        },

        horizontalAlignment: "center",
        verticalAlignment: "middle",
        yoffset: 10
    };

    // Create marker graphic
    const markerGraphic = new Graphic({
        geometry: point,
        symbol: markerSymbol
    });


    // Create a graphic
    const pointGraphic = new Graphic({
        geometry: point,
        symbol: textSymbolMarketValue,
        attributes: {
            marketValue: formattedValue,
            
        },
        popupTemplate: {
            title: "{description}",
            content: "Market Value: {formattedValue}</br></br/> Rating Category: {ratingCategory}",
        },
    });

   

    // Get all GraphicsLayers from the map
    const graphicsLayers = map.layers.filter(layer => layer.type === "graphics");
    // Get the first one (if any exist)
    const valuesGraphicsLayer = graphicsLayers.getItemAt(0);
    // Add graphic to the map's view
    valuesGraphicsLayer.graphics.addMany([markerGraphic,pointGraphic]);
}
// Dynamically load config.js if needed

function loadConfig() {
    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = "/config.js";
        script.onload = () => {
            if (window.__config) {
                resolve(window.__config);
                console.log("Config loaded successfully");
            } else {
                reject(new Error("Config not found"));
            }
        };
        script.onerror = () => reject(new Error("Failed to load config.js"));
        document.head.appendChild(script);
    });
}

