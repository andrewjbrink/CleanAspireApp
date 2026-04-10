
//Calcite Icons
/*https://developers.arcgis.com/calcite-design-system/icons/?query=*/


import { addBaseLayerToMap } from "./addSGlayers.js";
import { logToDotNet } from "./DotnetLogger.js";

const [esriConfig, Map, MapView, reactiveUtilsModule, TileLayer] =
    await $arcgis.import([
        "@arcgis/core/config.js",
        "@arcgis/core/Map.js",
        "@arcgis/core/views/MapView.js",
        "@arcgis/core/core/reactiveUtils.js",
        "@arcgis/core/layers/TileLayer.js",

    ]);

const [whenTrue, whenFalseOnce,] =
    await $arcgis.import(["@arcgis/core/core/reactiveUtils.js"]);

function showStatus(msg) {
    document.getElementById("status").innerText = msg;
}

function getFeatureLayerByTitle(view, title) {
    return view.map.layers.find(
        layer => layer.type === "feature" && layer.title === title
    );
}

async function getFeatureLayers(view) {
    const featureLayers = [];
    view.map.layers.forEach((layer) => {
        if (layer.type == 'feature') {
            //console.log("Found feature layer:", layer.title);
            featureLayers.push(layer);
        }
    })

}
//If you have group layers, you’ll want a recursive search:
function findFeatureLayerByTitleRecursive(layerCollection, title) {
    for (const layer of layerCollection.items) {
        if (layer.type === "feature" && layer.title === title) {
            return layer;
        }

        if (layer.type === "group") {
            const found = findFeatureLayerByTitleRecursive(layer.layers, title);
            if (found) return found;
        }
    }
    return null;
}

export async function findMunicipality(point) {
    const layer =  getFeatureLayerByTitle(view, "Municipalities");

    const query = layer.createQuery();
    query.geometry = point;
    query.spatialRelationship = "intersects";
    query.returnGeometry = false;
    query.outFields = ["*"];

    const result = await layer.queryFeatures(query);

    if (result.features.length > 0) {
        console.log("Municipality:", result.features[0].attributes.MUNICNAME);
        const municipalityName = result.features[0].attributes.MUNICNAME;
        return municipalityName;
    } else {

        console.log("No municipality found from spatial query");
    }
}

export function createSchemeUnitsUI(view)
{
    // Create the expander
    const expander = document.createElement("arcgis-expand");
    expander.expandIcon = "table";
    expander.expandTooltip = "Show Scheme Units";
    expander.collapseTooltip = "Hide Scheme Units";
    // Create the container div
    const container = document.createElement("div");
    container.id = "UnitsTableContainer";
    container.style.padding = "8px";
    container.style.maxHeight = "300px";
    container.style.overflowY = "auto";
    // Add your table HTML
    container.innerHTML = `
        <table id="ssUnitsTable" class="esri-widget">
            <thead>
                <tr>
                    <th>Property Reference</th>
                    <th>Address</th>
                    <th>Description</th>
                    <th>Rating Category</th>
                    <th>Extent</th>
                    <th>Market Value</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    `;
    // Append the container to the expander
    expander.appendChild(container);
    // Add the expander to the map UI
    view.ui.add(expander, { position: "bottom-left", index: 1 });

}

export function createCustomUI(view) {
    // Create the expander
    const expander = document.createElement("arcgis-expand");
    expander.expandIcon = "table";
    expander.expandTooltip = "Show Sales";
    expander.collapseTooltip = "Hide Sales";

    // Create the container div
    const container = document.createElement("div");
    container.id = "myTableContainer";
    container.style.padding = "8px";
    container.style.maxHeight = "300px";
    container.style.overflowY = "auto";
    // Add your table HTML
    container.innerHTML = `
     
        <table id="unitsTable" class="address-table">
            <thead>
                <tr>
                    <th>Property Reference</th>
                    <th>Address</th>
                    <th>Description</th>
                    <th>Erf Extent</th>
                    <th>Dwelling Extent</th>
                    <th>Sale Date</th>
                    <th>Sale Price</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    
    `;

    // Append the container to the expander
    expander.appendChild(container);
    // Add the expander to the map UI
    view.ui.add(expander, { position: "bottom-left", index: 0 });

}

export function showLoader() {
    window.loader.active = true;
}

export function hideLoader() {
    window.loader.active = false;
}


/**
 * Initialize and configure the ArcGIS map with custom layers
 
 */
async function initializeMap() {
    try {
        let centreLat = null;
        let centreLon = null;
        var lastLocation = localStorage.getItem('lastLocation');
        if (lastLocation) {
            var locationdata = lastLocation ? JSON.parse(lastLocation) : null;
            console.log("Last location from localStorage:", locationdata);
            centreLat =  -33.87712377738507;
            centreLon =  18.57555362304821;

            //centreLat = locationdata.latitude ?? -33.87712377738507;
            //centreLon = locationdata.longitude ?? 18.57555362304821;

            console.log(`Using last location - Latitude: ${centreLat}, Longitude: ${centreLon}`);
        }

        const reactiveUtils = reactiveUtilsModule?.default || reactiveUtilsModule;
        const apiKey =
            "AAPK82b104a218824d5ab511137e2c09663cYe0SYYzrnpV2qfoV4Q85nIq41mw5nUo-kt0kRALo-ITdxPT35cATLrKmGbMDqA7z";
        esriConfig.apiKey = apiKey;

        // Create the map with a basemap
        const map = new Map({
            basemap: "open/streets", // Or use your WebMap item ID  "arcgis/imagery"
        });

        // Create a view
        const view = new MapView({
            container: "viewDiv", // Reference to the DOM node that will contain the view
            map: map,
            zoom: 10,
            center: [centreLon, centreLat], // Longitude, latitude
        });





        //// Create loader
        const loader = document.createElement("calcite-loader");
        loader.type = "indeterminate";
        loader.scale = "l";
        loader.active = false; // start inactive

        // Position it
        loader.style.position = "fixed";
        loader.style.top = "50%";
        loader.style.left = "50%";
        loader.style.transform = "translate(-50%, -50%)";
        loader.style.zIndex = "9999";

        // Add to DOM
        document.body.appendChild(loader);
        //// IMPORTANT: turn it off AFTER attaching
        //requestAnimationFrame(() => {
        //    loader.active = false;
        //});
        // Store globally
        window.globalLoader = loader;
        document.querySelector("calcite-loader").hidden = true;
       
        const compassComponent = document.createElement("arcgis-compass");
        compassComponent.view = view;
        view.ui.add(compassComponent, { position: "top-left", index: 1 });

        const homeComponent = document.createElement("arcgis-home");
        homeComponent.view = view;
        view.ui.add(homeComponent, { position: "top-left", index: 2 });



        const layerListComponent = document.createElement("arcgis-layer-list")
        layerListComponent.view = view;
        layerListComponent.setAttribute("show-collapse-button", "false");
        layerListComponent.setAttribute("drag-enabled", "true");
        layerListComponent.setAttribute("show-heading", "true");
        layerListComponent.setAttribute("show-filter", "true");
        layerListComponent.setAttribute("collapsed", "true");
        layerListComponent.setAttribute("filter-placeholder", "Filter layers");
        view.ui.add(layerListComponent, "top-right");


        //create an expander for the basemap layers
        const expanderBasemaps = document.createElement("arcgis-expand");
        expanderBasemaps.expandIcon = "layers-list";
        expanderBasemaps.expandTooltip = "Show Basemaps";
        expanderBasemaps.collapseTooltip = "Hide Basemaps";


        const baseMapsComponent = document.createElement("arcgis-basemap-gallery")
        baseMapsComponent.view = view;

        const baseMapToggleComponent = document.createElement("arcgis-basemap-toggle")
        baseMapToggleComponent.view = view;
        baseMapToggleComponent.nextBasemap = "arcgis/imagery"; 
        expanderBasemaps.appendChild(baseMapsComponent);
        view.ui.add(baseMapToggleComponent, "top-left");


      


        // Create the container div
        const containerInfo = document.createElement("div");
        containerInfo.id = "infoContainer";
        containerInfo.style.padding = "8px";
        containerInfo.style.maxHeight = "300px";
        containerInfo.style.overflowY = "auto";
        containerInfo.innerHTML = `
                                <div id="info">
                                    <b>Map View Events</b><br />
                                    <div id="messages"></div>
                                </div>
                            `;

        //Add an expander at the bottom for information.
        const infoExpander = document.createElement("arcgis-expand");
        infoExpander.expandIcon = "geoevent-server";
        infoExpander.expandTooltip = "Show events";
        infoExpander.collapseTooltip = "Hide events";
        infoExpander.appendChild(containerInfo);
       /* view.ui.add(infoExpander, { position: "bottom-right", index: 0 });*/




        const printComponent = document.createElement("arcgis-print");
        printComponent.view = view;
        view.ui.add(printComponent, { position: "bottom-right", index: 0 });

        const printExpander = document.createElement("arcgis-expand");
        printExpander.expandIcon = "print";
        printExpander.expandTooltip = "Show print";
        printExpander.collapseTooltip = "Hide print";
        printExpander.appendChild(printComponent);
        /*view.ui.add(printExpander, { position: "bottom-right", index: 0 });*/

        const messagesDiv = containerInfo.querySelector("#messages");


        const state = {
            ready: false,
            zoom: null,
            scale: null,
            stationary: null,
            center: null,
            basemap: null,
            xmin: null,
            xmax: null,
            ymin: null,
            ymax: null
        };

        const mySources = await addBaseLayerToMap(map, view);

                // Add Search web component with sources
        const searchComponent = document.createElement("arcgis-search");
        searchComponent.view = view;
        searchComponent.sources = mySources;
        view.ui.add(searchComponent, { position: "top-left", index: 0 });

        
        //document.getElementById("messages").innerText = logElement;
        const renderState = () => {
            if (!messagesDiv) return; 
            messagesDiv.innerHTML = `
                                      <span>ready</span>: ${state.ready}<br/>
                                      <span>zoom</span>: ${state.zoom ?? "—"}<br/>
                                      <span>scale</span>: ${state.scale ?? "—"}<br/>
                                      <span>stationary</span>: ${state.stationary ?? "—"}<br/>
                                      <span>basemap</span>: ${state.basemap ?? "—"}
                                 `;
        };


        reactiveUtils.watch(
            () => [view.zoom, view.scale, view.stationary],
            ([zoomLevel, scaleValue, isStationary]) => {
                state.zoom = zoomLevel;
                state.scale = Math.round(scaleValue);
                state.stationary = isStationary;
                renderState();
                /*console.log(`Zoom: ${state.zoom}, Scale: ${state.scale}, Stationary: ${state.stationary}`);*/
            },
            { initial: true }
        );

        reactiveUtils.watch(
            () => map.basemap?.title,
            (basemapTitle) => {
                state.basemap = basemapTitle;
                renderState();
            },
            { initial: true },
        );

        reactiveUtils.watch(
            () => view.ready,
            (isComponentReady) => {
                state.ready = isComponentReady;
                renderState();
            },
            { initial: true },
        );

        //view.on("click", async event => {
        //    const point = event.mapPoint;
        //    locationElement.innerText = `Longitude: ${point.longitude.toFixed(6)}  |  Latitude: ${point.latitude.toFixed(6)}`;
        //    const response = await view.hitTest(event);
        //    const imageryLayer = response.results.find(r => r.layer.title === "World Imagery with Metadata");
        //    if (imageryLayer) {
        //        const attributes = imageryLayer.graphic.attributes;
        //        console.log("Imagery Date:", attributes.AcquisitionDate);
        //        console.log("Source:", attributes.Source);
        //        // Display in UI
        //       /* logToDotNet(`Imagery Date: ${attributes.AcquisitionDate}, Source: ${attributes.Source}`);*/
        //    }
        //});



        //https://community.esri.com/t5/arcgis-javascript-maps-sdk-questions/layerlist-wdiget-listen-to-layer-changes/td-p/1297056
        //reactiveUtils.watch(
        //    () => view.map.allLayers.map((layer) => layer.title),
        //    (titles) => {
        //        const areTitlesEqual = detectChanges(previousTitles, titles);
        //        if (!areTitlesEqual) {
        //            whatChanged(previousTitles, titles);
        //        }
        //        previousTitles = titles;
        //    }
        //);




        // Listen to click events on the view
        view.on("click", async (event) => {
            console.log("View clicked at:", {
                longitude: event.mapPoint.longitude,
                latitude: event.mapPoint.latitude,
            });

            localStorage.setItem('lastLocation', JSON.stringify({
                longitude: event.mapPoint.longitude,
                latitude: event.mapPoint.latitude
            }));
        });

        view.on("double-click", (event) => {
            console.log("Double-clicked at:", event.mapPoint);
        });

        view.on("drag", (event) => {
            console.log("Dragging...");
        });

        view.on("hold", (event) => {
            console.log("View held at:", {
                longitude: event.mapPoint.longitude,
                latitude: event.mapPoint.latitude,
            });
        });

        view.on("key-down", (event) => {
            console.log("Key pressed:", event.key);
        });

        view
            .when(() => {
                //Create the expander with the table for sales data
                createCustomUI(view);
                createSchemeUnitsUI(view);
                getFeatureLayers(view);
                
            })
            .catch((error) => {
                console.error("Error loading map view:", error);
            });

        console.log("Map initialized successfully");
        return { map, view };


    } catch (error)
        {
        console.error("Error initializing map:", error);
        throw error;
    }
}

// Initialize the map when the module loads
export { initializeMap };


