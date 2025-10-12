export enum eCurrency {
  USD = 'USD',
  EUR = 'EUR',
}

export function getECurrencyFromString(str: string): eCurrency | null {
  switch (str.toUpperCase()) {
    case 'USD':
      return eCurrency.USD;
    case 'EUR':
      return eCurrency.EUR;
    default:
      return null;
  }
}