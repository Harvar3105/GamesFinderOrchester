import { ECurrency } from "../enums/eCurrency";

export interface PriceRange {
  initial: number | null;
  current: number | null;
  currency: ECurrency | null;
}