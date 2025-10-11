import fetch from 'node-fetch';
import { Game } from '../entities/game';
import { v4 } from 'uuid';

export async function fetchSteamGame(id: number): Promise<Game | null> {
  const url = `https://store.steampowered.com/api/appdetails?appids=${id}&cc=us&l=en`;
  const res = await fetch(url);
  const data: any = await res.json(); //TODO: Type this properly

  if (!data[id]?.success) return null;
  const game = data[id].data;

  return {
    id: v4(),
    createdAt: new Date().toUTCString(),
    updatedAt: new Date().toUTCString(),
    name: game.name,
    steamUrl: `https://store.steampowered.com/app/${id}`,
    steamID: id,
    inPackages: game.packages || [],
    isDLC: game.required_product ? true : false,
    description: game.short_description || null,
    headerImage: game.header_image || null,
    price: {
      initial: game.price_overview ? game.price_overview.initial / 100 : null,
      current: game.price_overview ? game.price_overview.final / 100 : null,
      currency: game.price_overview ? game.price_overview.currency : null,
    }
  };
}

// export async function scrapeBatch(ids: number[], maxRequests: number, cooldownMs: number): Promise<Game[]> {
//   const results: Game[] = [];

//   for (let i = 0; i < ids.length; i += maxRequests) {
//     const batch = ids.slice(i, i + maxRequests);
//     const batchResults = await Promise.all(batch.map(fetchSteamGame));
//     results.push(...batchResults.filter(g => g !== null));

//     if (i + maxRequests < ids.length) {
//       console.log(`Cooldown ${cooldownMs / 1000} seconds due to API limit...`);
//       await new Promise(res => setTimeout(res, cooldownMs));
//     }
//   }

//   return results;
// }

export async function scrapeBatch(ids: number[]): Promise<Game[]> {
  const results: Game[] = [];

  results.push(
    ...(await Promise.all(
      ids.map(fetchSteamGame)
    ))
    .filter(g => g !== null) as Game[]
  );

  return results;
}
