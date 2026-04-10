export function downloadSales(sales) {
    var blob = new Blob([JSON.stringify(sales, null, 2)], { type: "application/json" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "sales.json";
    link.click();
}

export function downloadUnits(units) {
    var blob = new Blob([JSON.stringify(units, null, 2)], { type: "application/json" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "units.json";
    link.click();
}

export function formatRand(value) {
    return new Intl.NumberFormat('en-ZA', {
        style: 'currency',
        currency: 'ZAR'
    }).format(value);
}