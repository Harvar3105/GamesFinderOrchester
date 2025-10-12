export var eCurrency;
(function (eCurrency) {
    eCurrency["USD"] = "USD";
    eCurrency["EUR"] = "EUR";
})(eCurrency || (eCurrency = {}));
export function getECurrencyFromString(str) {
    switch (str.toUpperCase()) {
        case 'USD':
            return eCurrency.USD;
        case 'EUR':
            return eCurrency.EUR;
        default:
            return null;
    }
}
