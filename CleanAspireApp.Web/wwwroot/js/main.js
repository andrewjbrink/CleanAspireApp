import { initializeMap } from './mapLoader.js';
let savedState = null;
let savedGraphics = null;

export async function initializeMapWrapper() {
    try {
        const { map, view } = await initializeMap();
        // Store objects globally
        window.map = map;
        window.view = view;






        if (savedState) {
            view.goTo(savedState);
        }
        /*//console.log('Map and view available as window.map and window.view');*/
        return true; 

    } catch (error) {
        console.error('Failed to initialize map:', error);
    }
   
}

export function saveMapState() {
    const view = window.view;
    
    if (!view) return;

    savedState = {
        center: view.center,
        zoom: view.zoom,
        rotation: view.rotation
    };
    console.log('Map state saved:', savedState);
}


export function restoreMapState() {
    const view = window.view;
    if (!view || !savedState) {
        console.log('No MapState yet.');
        return;
    }
    view.goTo({
        center: savedState.center,
        zoom: savedState.zoom,
        rotation: savedState.rotation
    });
    console.log('Map state restored');
}

export function clearMapState() {
    savedState = null;
}


export function saveGraphics() {
    const view = window.view;
    if (!view) return;

    const graphics = view.graphics.toArray();

    // Convert graphics to JSON
    savedGraphics = graphics.map(g => g.toJSON());
}

export function restoreGraphics() {
    const view = window.view;
    if (!view || !savedGraphics) return;

    // Clear existing graphics
    view.graphics.removeAll();

    // Rebuild graphics from JSON
    savedGraphics.forEach(gJson => {
        const graphic = new window.arcgis.Graphic(gJson);
        view.graphics.add(graphic);
    });
}
