import { getMarketValue } from "./webApiClient.js";
import { getFarmMarketValue } from "./webApiClient.js";
import { getSSValuation } from "./webApiClient.js";
import { findMunicipality } from "./mapLoader.js";
import { logToDotNet } from "./DotnetLogger.js";
import { formatRand } from "./fileUtils.js";

const [CentroidOperator, webMercatorUtils] = await $arcgis.import([
    "@arcgis/core/geometry/operators/centroidOperator.js",
    "@arcgis/core/geometry/support/webMercatorUtils",
]);



//https://www.bing.com/search?q=calcite+table+in+a+popuptemplate&form=ANNTH1&refig=69ca4022069c4957a48e428f07499f48&pc=U531

//Check here for fonts
//https://developers.arcgis.com/javascript/latest/labeling/

//Property View Layers

//https://csggis.drdlr.gov.za/server/rest/services/Property_Viewer/MapServer

//Sectional Title
//https://csggis.drdlr.gov.za/server/rest/services/Property_Viewer/MapServer/604

function getFeatureLayerByTitle(view, title) {
    return view.map.layers.find(
        layer => layer.type === "feature" && layer.title === title
    );
}


export async function addBaseLayerToMap(map, view) {
    const [FeatureLayer, SimpleFillSymbol, SimpleRenderer, SimpleLineSymbol, reactiveUtilsModule, GraphicsLayer] =
        await $arcgis.import([
            "@arcgis/core/layers/FeatureLayer.js",
            "@arcgis/core/symbols/SimpleFillSymbol.js",
            "@arcgis/core/renderers/SimpleRenderer.js",
            "@arcgis/core/symbols/SimpleLineSymbol.js",
            "@arcgis/core/core/reactiveUtils.js",
            "@arcgis/core/layers/GraphicsLayer.js",
            
        ]);

    // normalize reactiveUtils default export (module comes back as {default: {...}})
    const reactiveUtils = reactiveUtilsModule?.default || reactiveUtilsModule;

    let rollData = null; // accessible globally (or in parent scope)

    reactiveUtils.on(
        () => view.popup,
        "trigger-action",
        async (event) => {
            if (event.action.id === "get-valuation-roll") {
                const selectedFeature = view.popup.selectedFeature;
               
                if (selectedFeature) {
                    //First get the municipality for the selected feature
                    const municipality = await findMunicipality(selectedFeature.geometry);

                    if (municipality) {
                        if (municipality === "City of Cape Town") {

                            rollData = await getMarketValue(selectedFeature, view, map);
                            
                            if (rollData) {
                                //console.log("Market Value Data:", rollData);
                                view.popup.content = formatRollData(rollData);
                            }
                        }
                        else
                        {
                            console.log("Only works for City of Cape Town ");
                        }
                    }
                }
            }
            if (event.action.id === "get-farm-valuation") {
                const selectedFeature = view.popup.selectedFeature;
                if (selectedFeature)
                {
                    const municipality = await findMunicipality(selectedFeature.geometry);
                    if (municipality) {
                        if (municipality === "City of Cape Town") {

                            rollData = await getFarmMarketValue(selectedFeature, view, map);
                            if (rollData) {
                                //console.log("Market Value Data:", rollData);
                                view.popup.content = formatRollData(rollData);
                                
                            }
                        }
                        else {
                            console.log("Only works for City of Cape Town ");
                        }
                    }
                }
            }
            if (event.action.id === "get-scheme-valuation") {
                const selectedFeature = view.popup.selectedFeature;
                if (selectedFeature) {
                    const municipality = await findMunicipality(selectedFeature.geometry);
                    if (municipality) {
                        if (municipality === "City of Cape Town") {

                            rollData = await getSSValuation(selectedFeature, view, map);
                            if (rollData && rollData.length > 0) {
                                console.log("Scheme unit count:", rollData.length);
                                //view.popup.content = formatRollData(rollData);
                                //This is going to return a list of all the units in the scheme, so we need to format it differently
                                //the user can then click on the unit they are interested in to get the details for that unit
                                //I will have to place this in a table format and then add an event listener to each row to update the popup content with the details for that unit

                            }
                        }
                        else {
                            console.log("Only works for City of Cape Town ");
                        }
                    }
                }
            }
        });

    function parseRandValue(value) {
        if (!value) return 0;

        // Remove "R", commas, and spaces
        const cleaned = value.replace(/R\s?|,/g, '');

        // Convert to number
        return Number(cleaned);
    }

    function formatRollData(data) {
        const hanging = data.ValuedTogether;
        let link = "";

        if (hanging && hanging.length > 0) {
            link = "Valued Together";
        } else
        {
            link = "";
        }
        
        const selectedFeature = view.popup.selectedFeature;
        const centroid = CentroidOperator.execute(selectedFeature.geometry);
        const wgs84Point = webMercatorUtils.webMercatorToGeographic(centroid);
        const formattedValue = formatRand(data.MarketValue);
        return `
        <div>
            <p><b>Address:</b> ${data.Address}</p>
            <p><b>Market Value:</b><h4> ${formattedValue}</h4></p>
            <p><b>Rating Category:</b> ${data.RatingCategory}</p>
            <p><b>Property Description:</b> ${data.Description}</p>
            <p>
                <a href="${data.Link}" target="_blank" rel="noopener noreferrer" style="color: blue; text-decoration: underline;">
                    CoCT Website
                </a>
            </p>

            
             <p>
                <a href="${data.HoldingLink}" target="_blank" rel="noopener noreferrer" style="color: blue; text-decoration: underline;">
                   ${link}
                </a>
            </p>

                <a href="https://www.google.com/maps?q=${wgs84Point.y},${wgs84Point.x}" target="_blank" rel="noopener noreferrer" style="color: blue; text-decoration: underline;">
                   View on Google Maps
                </a>
            <p>

            </p>
        </div>
               `;
    }

    function showStatus(msg) {
        document.getElementById("status").innerText = msg;
    }

    //view.on("pointer-move", (event) => {

    //    const message = `Pointer moved to: ${event.x}, ${event.y}`;
    //    showStatus(message);
    // });

    //Erven


    //https://developers.arcgis.com/javascript/latest/esri-icon-font/

    //create a line renderer for the cadastral dta to keep t hem all the same
    const cadastralRenderer = new SimpleRenderer({
        symbol: new SimpleLineSymbol({
            color: "yellow",
            width: 1,
            style: "solid",
        }),
    });

    const customAction = {
        title: "Valuation",
        id: "get-valuation-roll",
        className: "esri-icon-home", 
    };

    const farmAction = {
        title: "Valuation",
        id: "get-farm-valuation",
        className: "esri-icon-home", 
    };

    const ssAction = {
        title: "Units",
        id: "get-scheme-valuation",
        className: "esri-icon-home",
    };

    const labelClass = {
        // autocasts as new LabelClass()
        symbol: {
            type: "text", // autocasts as new TextSymbol()
            color: "white",
            haloColor: "blue",
            haloSize: 1,
            font: {
                family: "Josefin Sans",
                style: "normal",
                weight: "normal",
                size: 12,
            },
        },
        labelPlacement: "above-right",
        labelExpressionInfo: {
            expression: "$feature.MarketValue",
        },
    };
    //where: "Conference = 'AFC'",
    //expression: "$feature.Team + TextFormatting.NewLine + $feature.Division",

    //Create a GraphicsLayer for market values
    const graphicsLayerMarketValues = new GraphicsLayer({
        title: "Market Values",
        labelingInfo: [labelClass],
    });
    map.add(graphicsLayerMarketValues);

    //Create a GraphicsLayer for sales
    const graphicsLayerSales = new GraphicsLayer({
        title: "Sales",
    });
    map.add(graphicsLayerSales);



    function zoomToFeature(view, geometry) {
        view.goTo({
            target: geometry,
            zoom: 15 // Adjust zoom level as needed
        }).catch(function (error) {
            console.error("Error zooming to feature:", error);
        });
    }


   

    const ervenRegistered = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/2",
        title: "Erven",
        visible: true,
        minScale: 20_000,
        outFields: ["*"],
        popupTemplate: {
            dockEnabled: true,
            title: "{ID}",
            content:
                "SGKEY: {ID} <br/> <br/> ERF DESCRIPTION: {TAG_VALUE} - {MIN_REGION}",
            actions: [customAction],
        },
    });
    ervenRegistered
        .load()
        .then(() => {
            map.add(ervenRegistered);
        })
        .catch((error) => { });




    const farmPortion = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/3",
        title: "Farm Portion",
        visible: true,
        minScale: 40_000,
        outFields: ["*"],
        popupTemplate: {
            dockEnabled: true,
            title: "{FARM_NAME}",
            content:
                "SGKEY: {ID} <br/> <br/> FARM:{PARCEL_NO} PTN:{PORTION} IN {MAJ_REGION}",
            actions: [farmAction],
        },
    });
    farmPortion
        .load()
        .then(() => {
            map.add(farmPortion);
            //logToDotNet(`Farm Portion FeatureLayer loaded successfully.`);
        })
        .catch((error) => { });


   
    //Sectional Title

    const ssPopupTemplate =
    {
        dockEnabled: true,
        title: "{ST_SCHM_NAME}",
        content: `
        <div>
              <label for="popup-input">Unit Number:</label>
              <calcite-input
                id="popup-input"
                class="popup-input"
                placeholder="Type here..."
                clearable
              ></calcite-input>
            </div>
        `,


    };


    const ssRenderer = new SimpleRenderer({
        symbol: new SimpleLineSymbol({
            color: "red",
            width: 1.2,
            style: "solid",
        }),
    });

    const ssSchemes = new FeatureLayer({
        url: "https://esapqa.capetown.gov.za/agsext/rest/services/Theme_Based/ODP_SPLIT_3/FeatureServer/6",
        title: "CoCT Sectional Title",
        visible: true,
        minScale: 70_000,
        outFields: ["*"],
        popupTemplate: {
            dockEnabled: true,
            title: "{ST_SCHM_NAME}",
            content:
                "SCHEME NUMBER: {ST_SCHM_NO} <br/><br/> SCHEME YEAR:{ST_SCHM_YEAR}<br/><br/> PLAN NUMBER:{PLAN_NO}<br/><br/> YEAR YEAR:{PLAN_YEAR}",
            actions: [ssAction],
        },
    });
    ssSchemes
        .load()
        .then(() => {
            map.add(ssSchemes);
        })
        .catch((error) => { });


    //Allotments
    const allotments = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/1",
        title: "Allotment Township",
        visible: false,
        minScale: 1100000,
        popupTemplate: {
            dockEnabled: true,
            title: "{TOWN_CODE}",
            content:
                "ALLOTMENT NAME: {TAG_VALUE} ",
        }
    });
    allotments
        .load()
        .then(() => {
            map.add(allotments);
        })
        .catch((error) => { });

    //Administrative Regions
    const adminRegions = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/47",
        title: "Administrative Regions",
        visible: true,
        minScale: 1100000,
    });
    adminRegions
        .load()
        .then(() => {
            map.add(adminRegions);
        })
        .catch((error) => { });

    //Municipalities
    //https://developers.arcgis.com/javascript/latest/tutorials/query-a-feature-layer-spatial/

    //create a label class
    const MunicipalityLabelClass = {
        labelExpressionInfo: {
            expression: "$feature.MUNICNAME ", 
        },
        symbol: {
            type: "text",
            color: "white",
            haloColor: "blue",
            haloSize: 1,
            font: {
                family: "Josefin Sans",
                style: "normal",
                weight: "normal",
                size: 12,
            },
        },
        minScale: 4000_000, // Visible when zoomed in closer than this
        maxScale: 10000, // Visible when zoomed out beyond this
    };

    //create a renderer
    const municipalityRenderer = new SimpleRenderer({
        symbol: new SimpleLineSymbol({
            color: "blue",
            width: 1.6,
            style: "solid",
        }),
    });

    const municipalities = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/40",
        title: "Municipalities",
        labelingInfo: [MunicipalityLabelClass],
        renderer: municipalityRenderer,
        visible: true,
        minScale: 10_000_000,
        popupTemplate: {
            dockEnabled: true,
            title: "{MUNICNAME}",
            content:
                "Municipality Name: {MUNICNAME} <br/> Province: {PROVINCE_NAME} <br/> Category: {CAT2}", //"$feature.PROVINCE + TextFormatting.NewLine + $feature.CODE"
        },
    });
    municipalities
        .load()
        .then(() => {
            map.add(municipalities);
        })
        .catch((error) => { });


   


    //Provinces
    const provinces = new FeatureLayer({
        url: "https://csggis.drdlr.gov.za/server/rest/services/CSGSearch/MapServer/14",
        title: "Provinces",
        visible: true,
        minScale: 70_000_000,
    });
    provinces
        .load()
        .then(() => {
            map.add(provinces);
        })
        .catch((error) => { });

    //setup the sources for the Search widget

    var mySources = [
        {
            layer: municipalities,
            searchFields: ["MUNICNAME"],
            displayField: "MUNICNAME",
            exactMatch: false,
            outFields: ["*"],
            name: "Municipalities",
            placeholder: "example: Langeberg",
            visible: true,
        },
        {
            layer: allotments,
            searchFields: ["TAG_VALUE"],
            displayField: "TAG_VALUE",
            exactMatch: false,
            outFields: ["*"],
            name: "Allotments",
            placeholder: "example: Ottery",
            visible: true,
        },
        {
            layer: ssSchemes,
            searchFields: ["ST_SCHM_NAME"],
            displayField: "ST_SCHM_NAME",
            exactMatch: false,
            outFields: ["*"],
            name: "Schemes",
            placeholder: "example: HAMPTON PLACE",
            visible: true,
        },
    ];

    return mySources;
}
