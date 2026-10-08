export const AVATAR_SIZE = 256;

/**
 * Center-crops an image to a square and scales it to `size` px, encoded as WebP (JPEG where the
 * browser cannot encode WebP). A phone photo of several MB becomes a ~20 KB upload.
 */
export async function toSquareImage(file: Blob, size = AVATAR_SIZE): Promise<Blob> {
  const bitmap = await createImageBitmap(file);
  const side = Math.min(bitmap.width, bitmap.height);
  const canvas = document.createElement('canvas');
  canvas.width = size;
  canvas.height = size;
  const context = canvas.getContext('2d');
  if (!context) throw new Error('Canvas is not available');
  context.imageSmoothingQuality = 'high';
  context.drawImage(bitmap, (bitmap.width - side) / 2, (bitmap.height - side) / 2, side, side, 0, 0, size, size);
  bitmap.close();

  const encode = (type: string) => new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, type, 0.88));
  const webp = await encode('image/webp');
  // Browsers that cannot encode WebP silently fall back to PNG; JPEG is much smaller for photos.
  const blob = webp?.type === 'image/webp' ? webp : await encode('image/jpeg');
  if (!blob) throw new Error('Could not encode the image');
  return blob;
}

/** Stable pleasant hue per name, for the letter avatar shown when there is no picture. */
export function avatarHue(name: string): number {
  let hash = 0;
  for (const char of name) hash = (hash * 31 + char.codePointAt(0)!) % 360;
  return hash;
}

/** One or two letters: "Alice Smith" -> "AS", "bob" -> "B". */
export function initials(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean);
  const letters = words.length > 1 ? [words[0]!, words[words.length - 1]!] : words.slice(0, 1);
  return letters.map((w) => [...w][0]!.toUpperCase()).join('') || '?';
}
