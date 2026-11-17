/* ============ Ember & Oak — products & cart ============ */

const PRODUCTS = [
  {
    id: "sandalwood-amber",
    name: "Sandalwood & Amber",
    collection: "warm",
    collectionLabel: "Warm & Woody",
    notes: "Sandalwood · Amber · Vanilla bean",
    price: 34,
    was: null,
    rating: 4.9,
    reviews: 412,
    burn: "60 hr",
    badge: "Bestseller",
    jar: "#5a483a",
    jarLight: "#7a6450",
    wax: "#f0e2cc",
  },
  {
    id: "cedar-smoke",
    name: "Cedar & Smoke",
    collection: "warm",
    collectionLabel: "Warm & Woody",
    notes: "Cedarwood · Vetiver · Clove",
    price: 32,
    was: null,
    rating: 4.8,
    reviews: 268,
    burn: "55 hr",
    badge: null,
    jar: "#3d3128",
    jarLight: "#5c4c3e",
    wax: "#ecdcc2",
  },
  {
    id: "tobacco-vanilla",
    name: "Tobacco & Vanilla",
    collection: "warm",
    collectionLabel: "Warm & Woody",
    notes: "Tonka · Tobacco leaf · Bourbon",
    price: 36,
    was: 42,
    rating: 4.9,
    reviews: 331,
    burn: "60 hr",
    badge: "Sale",
    jar: "#4a2f22",
    jarLight: "#6b4a36",
    wax: "#f2e4cd",
  },
  {
    id: "oakmoss-musk",
    name: "Oakmoss & Musk",
    collection: "warm",
    collectionLabel: "Warm & Woody",
    notes: "Oakmoss · White musk · Patchouli",
    price: 32,
    was: null,
    rating: 4.7,
    reviews: 154,
    burn: "55 hr",
    badge: null,
    jar: "#2f3a2c",
    jarLight: "#4a5845",
    wax: "#eee0c8",
  },
  {
    id: "jasmine-sea-salt",
    name: "Jasmine & Sea Salt",
    collection: "fresh",
    collectionLabel: "Fresh & Floral",
    notes: "Night jasmine · Sea salt · Driftwood",
    price: 34,
    was: null,
    rating: 4.8,
    reviews: 297,
    burn: "60 hr",
    badge: "Bestseller",
    jar: "#2e4a52",
    jarLight: "#48707a",
    wax: "#f6eedd",
  },
  {
    id: "fig-leaf",
    name: "Fig Leaf & Cassis",
    collection: "fresh",
    collectionLabel: "Fresh & Floral",
    notes: "Green fig · Blackcurrant · Bamboo",
    price: 30,
    was: null,
    rating: 4.7,
    reviews: 189,
    burn: "50 hr",
    badge: "New",
    jar: "#3c4a35",
    jarLight: "#5a6e50",
    wax: "#f4ead6",
  },
  {
    id: "white-tea-ginger",
    name: "White Tea & Ginger",
    collection: "fresh",
    collectionLabel: "Fresh & Floral",
    notes: "White tea · Ginger · Bergamot",
    price: 30,
    was: null,
    rating: 4.6,
    reviews: 142,
    burn: "50 hr",
    badge: null,
    jar: "#51585e",
    jarLight: "#757d84",
    wax: "#f7f0e0",
  },
  {
    id: "spiced-chestnut",
    name: "Spiced Chestnut",
    collection: "seasonal",
    collectionLabel: "Seasonal",
    notes: "Roasted chestnut · Cinnamon · Clove",
    price: 36,
    was: null,
    rating: 4.9,
    reviews: 376,
    burn: "60 hr",
    badge: "Limited",
    jar: "#6b3a24",
    jarLight: "#8d5a3c",
    wax: "#f0dfc4",
  },
  {
    id: "cranberry-fir",
    name: "Cranberry & Fir",
    collection: "seasonal",
    collectionLabel: "Seasonal",
    notes: "Cranberry · Douglas fir · Orange peel",
    price: 34,
    was: 40,
    rating: 4.8,
    reviews: 243,
    burn: "55 hr",
    badge: "Sale",
    jar: "#5a2430",
    jarLight: "#7e4150",
    wax: "#f3e6d2",
  },
  {
    id: "pumpkin-chai",
    name: "Pumpkin & Chai",
    collection: "seasonal",
    collectionLabel: "Seasonal",
    notes: "Pumpkin · Chai spice · Brown sugar",
    price: 32,
    was: null,
    rating: 4.7,
    reviews: 208,
    burn: "55 hr",
    badge: null,
    jar: "#7a4a26",
    jarLight: "#9c6c44",
    wax: "#f1e1c6",
  },
  {
    id: "midnight-birch",
    name: "Midnight Birch",
    collection: "seasonal",
    collectionLabel: "Seasonal",
    notes: "Birch tar · Pine · Frozen air",
    price: 36,
    was: null,
    rating: 4.9,
    reviews: 176,
    burn: "60 hr",
    badge: "Limited",
    jar: "#232c33",
    jarLight: "#3d4a54",
    wax: "#eef0ea",
  },
  {
    id: "honey-oat",
    name: "Honey & Oat Milk",
    collection: "fresh",
    collectionLabel: "Fresh & Floral",
    notes: "Wildflower honey · Oat milk · Almond",
    price: 30,
    was: null,
    rating: 4.8,
    reviews: 221,
    burn: "50 hr",
    badge: "New",
    jar: "#b98a3e",
    jarLight: "#d4ab63",
    wax: "#f8f1e0",
  },
];

const FREE_SHIPPING_THRESHOLD = 60;

/* ---------- State ---------- */
let cart = {};
let activeFilter = "all";

/* ---------- Helpers ---------- */
const $ = (sel) => document.querySelector(sel);
const money = (n) => `$${n.toFixed(2)}`;

function candleSVG(p, small = false) {
  const label = small ? "" : `<text x="100" y="128" text-anchor="middle" font-family="Georgia, serif" font-size="11" fill="#4a3c2e" letter-spacing="2">EMBER &amp; OAK</text>`;
  return `
  <svg viewBox="0 0 200 210" aria-hidden="true">
    <defs>
      <linearGradient id="jar-${p.id}-${small}" x1="0" y1="0" x2="1" y2="0">
        <stop offset="0" stop-color="${p.jar}"/>
        <stop offset="0.5" stop-color="${p.jarLight}"/>
        <stop offset="1" stop-color="${p.jar}"/>
      </linearGradient>
      <radialGradient id="fl-${p.id}-${small}" cx="0.5" cy="0.7" r="0.6">
        <stop offset="0" stop-color="#fff7d6"/>
        <stop offset="0.55" stop-color="#ffc94d"/>
        <stop offset="1" stop-color="#e8862e" stop-opacity="0"/>
      </radialGradient>
    </defs>
    <ellipse cx="100" cy="200" rx="72" ry="7" fill="#2b241d" opacity="0.12"/>
    <rect x="52" y="98" width="96" height="98" rx="9" fill="url(#jar-${p.id}-${small})"/>
    <rect x="59" y="105" width="7" height="84" rx="3.5" fill="#fff" opacity="0.1"/>
    <rect x="55" y="88" width="90" height="16" rx="5" fill="${p.wax}"/>
    <path d="M100 44 c-6 11 -13 17 -13 26 a13 13 0 0 0 26 0 c0-9 -7-15 -13-26z" fill="url(#fl-${p.id}-${small})"/>
    <path d="M100 57 c-3.5 6 -7 10 -7 14 a7 7 0 0 0 14 0 c0-4 -3.5-8 -7-14z" fill="#ffd66b"/>
    <rect x="98.5" y="82" width="3" height="10" rx="1.5" fill="#7a6a55"/>
    ${small ? "" : `<rect x="44" y="120" width="112" height="34" rx="4" fill="#f4ecdf" stroke="#d9cbb4"/>`}
    ${label}
  </svg>`;
}

function stars(rating) {
  const full = Math.round(rating);
  return "★".repeat(full) + "☆".repeat(5 - full);
}

/* ---------- Render products ---------- */
function renderProducts() {
  const grid = $("#productGrid");
  grid.innerHTML = PRODUCTS.map((p) => {
    const badge = p.badge
      ? `<span class="badge ${p.badge === "Sale" ? "sale" : p.badge === "New" ? "new" : ""}">${p.badge}</span>`
      : "";
    const was = p.was ? `<span class="was">${money(p.was)}</span>` : "";
    return `
    <article class="product-card" data-collection="${p.collection}">
      <div class="product-media">
        ${badge}
        ${candleSVG(p)}
      </div>
      <div class="product-body">
        <p class="product-collection">${p.collectionLabel} · ${p.burn} burn</p>
        <h3 class="product-name">${p.name}</h3>
        <p class="product-notes">${p.notes}</p>
        <p class="product-rating">${stars(p.rating)} <span>${p.rating} (${p.reviews})</span></p>
        <div class="product-foot">
          <span class="price">${money(p.price)}${was}</span>
          <button class="add-btn" data-add="${p.id}">Add to cart</button>
        </div>
      </div>
    </article>`;
  }).join("");
}

/* ---------- Cart ---------- */
function cartEntries() {
  return Object.entries(cart)
    .map(([id, qty]) => ({ product: PRODUCTS.find((p) => p.id === id), qty }))
    .filter((e) => e.product);
}

function cartCount() {
  return cartEntries().reduce((sum, e) => sum + e.qty, 0);
}

function cartSubtotal() {
  return cartEntries().reduce((sum, e) => sum + e.product.price * e.qty, 0);
}

function saveCart() {
  localStorage.setItem("emberOakCart", JSON.stringify(cart));
}

function loadCart() {
  try {
    cart = JSON.parse(localStorage.getItem("emberOakCart")) || {};
  } catch {
    cart = {};
  }
}

function addToCart(id) {
  cart[id] = (cart[id] || 0) + 1;
  saveCart();
  renderCart();
  const p = PRODUCTS.find((x) => x.id === id);
  toast(`✓ ${p.name} added to cart`);
}

function setQty(id, qty) {
  if (qty <= 0) delete cart[id];
  else cart[id] = qty;
  saveCart();
  renderCart();
}

function renderCart() {
  const count = cartCount();
  $("#cartCount").textContent = count;
  $("#cartCount").style.display = count ? "grid" : "none";

  const itemsEl = $("#cartItems");
  const entries = cartEntries();

  if (!entries.length) {
    itemsEl.innerHTML = `
      <div class="cart-empty">
        <div class="big">🕯️</div>
        <p><strong>Your cart is empty</strong></p>
        <p style="font-size:.86rem">Your next favorite scent is waiting.</p>
      </div>`;
    $("#cartFoot").style.display = "none";
    return;
  }

  $("#cartFoot").style.display = "block";
  itemsEl.innerHTML = entries
    .map(({ product: p, qty }) => {
      const bg = p.jarLight;
      return `
      <div class="cart-item">
        <div class="cart-item-thumb" style="background:${p.wax}">${candleSVG(p, true)}</div>
        <div>
          <p class="cart-item-name">${p.name}</p>
          <p class="cart-item-price">${money(p.price)} each</p>
          <div class="qty-controls">
            <button class="qty-btn" data-dec="${p.id}" aria-label="Decrease quantity">−</button>
            <span class="qty-val">${qty}</span>
            <button class="qty-btn" data-inc="${p.id}" aria-label="Increase quantity">+</button>
          </div>
        </div>
        <div>
          <p class="cart-item-total">${money(p.price * qty)}</p>
          <button class="remove-btn" data-remove="${p.id}">Remove</button>
        </div>
      </div>`;
    })
    .join("");

  const subtotal = cartSubtotal();
  $("#cartTotal").textContent = money(subtotal);
  const note = $("#cartNote");
  if (subtotal >= FREE_SHIPPING_THRESHOLD) {
    note.textContent = "🎉 You've unlocked free shipping!";
    note.classList.add("free");
  } else {
    note.textContent = `You're ${money(FREE_SHIPPING_THRESHOLD - subtotal)} away from free shipping`;
    note.classList.remove("free");
  }
}

/* ---------- Toast ---------- */
let toastTimer;
function toast(msg) {
  const el = $("#toast");
  el.textContent = msg;
  el.classList.add("show");
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => el.classList.remove("show"), 2200);
}

/* ---------- Cart drawer open/close ---------- */
function openCart() {
  $("#cartDrawer").classList.add("open");
  $("#cartOverlay").classList.add("open");
  document.body.style.overflow = "hidden";
}
function closeCart() {
  $("#cartDrawer").classList.remove("open");
  $("#cartOverlay").classList.remove("open");
  document.body.style.overflow = "";
}

/* ---------- Events ---------- */
document.addEventListener("click", (e) => {
  const add = e.target.closest("[data-add]");
  if (add) addToCart(add.dataset.add);

  const inc = e.target.closest("[data-inc]");
  if (inc) setQty(inc.dataset.inc, (cart[inc.dataset.inc] || 0) + 1);

  const dec = e.target.closest("[data-dec]");
  if (dec) setQty(dec.dataset.dec, (cart[dec.dataset.dec] || 0) - 1);

  const rem = e.target.closest("[data-remove]");
  if (rem) setQty(rem.dataset.remove, 0);

  const filter = e.target.closest(".filter");
  if (filter) {
    activeFilter = filter.dataset.filter;
    document.querySelectorAll(".filter").forEach((f) => f.classList.toggle("active", f === filter));
    document.querySelectorAll(".product-card").forEach((card) => {
      const show = activeFilter === "all" || card.dataset.collection === activeFilter;
      card.classList.toggle("hidden", !show);
    });
  }

  const collection = e.target.closest(".collection-card");
  if (collection) {
    const key = collection.dataset.collection;
    const btn = document.querySelector(`.filter[data-filter="${key}"]`);
    if (btn) btn.click();
  }
});

$("#cartBtn").addEventListener("click", openCart);
$("#cartClose").addEventListener("click", closeCart);
$("#cartOverlay").addEventListener("click", closeCart);

$("#checkoutBtn").addEventListener("click", () => {
  toast("Demo store — checkout isn't wired to a payment gateway.");
});

$("#menuBtn").addEventListener("click", () => {
  $("#mobileNav").classList.toggle("open");
});
document.querySelectorAll("#mobileNav a").forEach((a) =>
  a.addEventListener("click", () => $("#mobileNav").classList.remove("open"))
);

$("#newsletterForm").addEventListener("submit", (e) => {
  e.preventDefault();
  const email = $("#newsletterEmail").value.trim();
  if (!email) return;
  $("#newsletterForm").hidden = true;
  $("#newsletterMsg").hidden = false;
});

window.addEventListener("scroll", () => {
  $("#header").classList.toggle("scrolled", window.scrollY > 8);
});

$("#searchBtn").addEventListener("click", () => {
  document.getElementById("shop").scrollIntoView({ behavior: "smooth" });
  toast("Browse the full collection below");
});

/* ---------- Init ---------- */
renderProducts();
loadCart();
renderCart();
