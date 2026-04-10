export function logToDotNet(message) {

    DotNet.invokeMethodAsync('CleanAspireApp', 'LogFromJS', message)
        .then(() => console.log('Logged to .NET successfully'))
        .catch(err => console.error('Interop logging failed:', err));
   
}
export function logFields(FeatureLayer) {
    const fields = FeatureLayer.fields;
    fields.forEach(field => {
        logToDotNet(`Field name: ${field.name}, Type: ${field.type}`);
    });
}