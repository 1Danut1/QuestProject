import { Product } from '../models/product';

/**
 * Curated stock photos (Pexels / Wikimedia) matched by product name keywords.
 * Fallback cycles by product id so each row stays consistent without random picsum.
 */
const IMAGES = {
  /** CPU / processor */
  cpu: 'https://images.pexels.com/photos/1432674/pexels-photo-1432674.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** RAM modules */
  ram: 'https://images.pexels.com/photos/1714205/pexels-photo-1714205.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** GPU / graphics card */
  gpu: 'https://images.pexels.com/photos/1029757/pexels-photo-1029757.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** SSD / HDD / storage */
  storage:
    'https://images.pexels.com/photos/1786430/pexels-photo-1786430.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Motherboard */
  motherboard:
    'https://images.pexels.com/photos/2582937/pexels-photo-2582937.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Power supply */
  psu: 'https://images.pexels.com/photos/442154/pexels-photo-442154.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** PC case / tower */
  case: 'https://images.pexels.com/photos/4709291/pexels-photo-4709291.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Cooling / fans */
  cooling: 'https://images.pexels.com/photos/442150/pexels-photo-442150.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Monitor / display */
  monitor: 'https://images.pexels.com/photos/1029759/pexels-photo-1029759.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Keyboard / mouse / peripherals */
  peripheral:
    'https://images.pexels.com/photos/2115257/pexels-photo-2115257.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
  /** Default: refurbished / workshop PC parts */
  default:
    'https://images.pexels.com/photos/2582937/pexels-photo-2582937.jpeg?auto=compress&cs=tinysrgb&w=640&h=480&fit=crop',
} as const;

const RULES: { keys: string[]; url: string }[] = [
  {
    keys: [
      'cpu',
      'procesor',
      'processor',
      'ryzen',
      'intel',
      'core i',
      'core-i',
      'xeon',
      'threadripper',
      'pentium',
      'celeron',
      'socket',
    ],
    url: IMAGES.cpu,
  },
  {
    keys: ['ram', 'ddr', 'dimm', 'memorie', 'memory', 'sodimm', 'ecc'],
    url: IMAGES.ram,
  },
  {
    keys: [
      'gpu',
      'video',
      'graphics',
      'rtx',
      'gtx',
      'radeon',
      'geforce',
      'nvidia',
      'quadro',
      'titan',
    ],
    url: IMAGES.gpu,
  },
  {
    keys: [
      'ssd',
      'hdd',
      'nvme',
      'm.2',
      'storage',
      'hard drive',
      'harddisk',
      'solid state',
    ],
    url: IMAGES.storage,
  },
  {
    keys: ['motherboard', 'mainboard', 'placa de baza', 'placă', 'mobo', 'chipset', 'b450', 'b550', 'x570', 'z690'],
    url: IMAGES.motherboard,
  },
  {
    keys: ['psu', 'power supply', 'sursa', 'sursă', 'watt', '80 plus', 'modular'],
    url: IMAGES.psu,
  },
  {
    keys: ['case', 'carcasa', 'carcasă', 'tower', 'chassis', 'cabinet', 'mid tower'],
    url: IMAGES.case,
  },
  {
    keys: ['cooler', 'cooling', 'fan', 'ventilator', 'aio', 'heatsink', 'radiator', 'racire', 'răcire'],
    url: IMAGES.cooling,
  },
  {
    keys: ['monitor', 'display', 'ecran', 'lcd', 'oled', 'ips', 'hz'],
    url: IMAGES.monitor,
  },
  {
    keys: ['keyboard', 'tastatura', 'tastatură', 'mouse', 'peripheral', 'headset', 'cască'],
    url: IMAGES.peripheral,
  },
];

const ROTATION = [
  IMAGES.cpu,
  IMAGES.ram,
  IMAGES.gpu,
  IMAGES.storage,
  IMAGES.motherboard,
  IMAGES.psu,
  IMAGES.case,
  IMAGES.cooling,
] as const;

export function getProductImageUrl(product: Product): string {
  const normalized = product.name.toLowerCase();

  for (const rule of RULES) {
    if (rule.keys.some((k) => normalized.includes(k))) {
      return rule.url;
    }
  }

  const idx = Math.abs(product.id) % ROTATION.length;
  return ROTATION[idx] ?? IMAGES.default;
}
