export enum eRegion { // Steam uses not region but country codes, but for simplicity we use regions. For EU works any EU country code
  US = 'us',
  EU = 'ee',
}

export function getERegionFromString(region: string): eRegion | null {
  switch (region.toLowerCase()) {
    case 'us':
      return eRegion.US;
    case 'eu':
      return eRegion.EU;
    default:
      return null;
  }
}