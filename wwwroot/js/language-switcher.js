(function () {
    // ── Remove any leftover Google Translate cookies to stop reload loop ──
    try {
        var domain = window.location.hostname;
        document.cookie = "googtrans=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        document.cookie = "googtrans=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; domain=" + domain + ";";
    } catch (e) { }

    var dictionary = {
        // Navigation & General
        "হোম": "Home",
        "প্রোডাক্ট": "Products",
        "সব প্রোডাক্ট": "All Products",
        "সব পণ্য": "All Products",
        "সব পণ্য দেখুন": "View All Products",
        "কম্বো অফার": "Combo Offers",
        "কার্ট": "Cart",
        "আমার কার্ট": "My Cart",
        "উইশলিস্ট": "Wishlist",
        "আমার উইশলিস্ট": "My Wishlist",
        "সাপোর্ট চ্যাট": "Support Chat",
        "আমার প্রোফাইল": "My Profile",
        "প্রোফাইল এডিট": "Edit Profile",
        "পাসওয়ার্ড পরিবর্তন": "Change Password",
        "আমার অর্ডার": "My Orders",
        "অ্যাডমিন প্যানেল": "Admin Panel",
        "লগআউট": "Logout",
        "লগইন": "Login",
        "রেজিস্টার": "Register",
        "ক্যাটাগরি": "Categories",
        "সব ক্যাটাগরি": "All Categories",
        "মেনু": "Menu",
        "যোগাযোগ": "Contact",
        "দ্রুত লিংক": "Quick Links",
        "সকল স্বত্ব সংরক্ষিত": "All Rights Reserved",
        "ঢাকা, বাংলাদেশ": "Dhaka, Bangladesh",
        "শনি–বৃহঃ, সকাল ৯টা – সন্ধ্যা ৬টা": "Sat–Thu, 9:00 AM – 6:00 PM",

        // Search & Filter
        "প্রোডাক্ট খুঁজুন...": "Search products...",
        "প্রোডাক্টের নাম খুঁজুন...": "Search product name...",
        "প্রোডাক্টের নাম...": "Product name...",
        "ফিল্টার": "Filter",
        "সার্চ": "Search",
        "সার্চ করুন": "Search",
        "রিসেট": "Reset",
        "মূল্য পরিসর": "Price Range",
        "মূল্য পরিসর (৳)": "Price Range (৳)",
        "সর্বনিম্ন": "Min",
        "সর্বোচ্চ": "Max",
        "সাজানো": "Sort by",
        "নতুন আগে": "Newest First",
        "মূল্য: কম থেকে বেশি": "Price: Low to High",
        "মূল্য: বেশি থেকে কম": "Price: High to Low",
        "নাম অনুযায়ী": "By Name",
        "সব স্ট্যাটাস": "All Status",
        "সক্রিয়": "Active",
        "নিষ্ক্রিয়": "Inactive",

        // Product & Stock
        "কার্টে যোগ": "Add to Cart",
        "কার্টে যোগ করুন": "Add to Cart",
        "কার্টে যোগ হয়েছে!": "Added to Cart!",
        "যোগ হয়েছে!": "Added!",
        "সাইজ দেখুন": "View Sizes",
        "সাইজ নির্বাচন করুন": "Select Size",
        "স্টকে আছে": "In Stock",
        "স্টক শেষ": "Out of Stock",
        "স্টক নেই": "Out of Stock",
        "কোড": "Code",
        "বিবরণ": "Description",
        "রিভিউ ও রেটিং": "Reviews & Ratings",
        "আপনার রিভিউ লিখুন": "Write Your Review",
        "রেটিং": "Rating",
        "আপনার অভিজ্ঞতা শেয়ার করুন...": "Share your experience...",
        "রিভিউ জমা দিন": "Submit Review",
        "এখনো কোনো রিভিউ নেই।": "No reviews yet.",
        "সম্পর্কিত পণ্য": "Related Products",
        "একক মূল্য": "Unit Price",
        "পরিমাণ": "Quantity",
        "থেকে": "from",
        "ছাড়": "Discount",
        "সাশ্রয়": "Save",
        "টি": " items",
        "টি প্রোডাক্ট": " Products",
        "কোনো প্রোডাক্ট পাওয়া যায়নি": "No products found",

        // Features & Banner
        "দ্রুত ডেলিভারি": "Fast Delivery",
        "ঢাকায় ১-২ দিন, বাইরে ৩-৫ দিন": "1-2 days in Dhaka, 3-5 days outside",
        "১০০% অরিজিনাল": "100% Original",
        "গ্যারান্টিযুক্ত পণ্য": "Guaranteed Products",
        "সহজ রিটার্ন": "Easy Returns",
        "৭ দিনের মধ্যে": "Within 7 days",
        "একসাথে কিনুন, বেশি সাশ্রয় করুন": "Buy together, save more",
        "এই মুহূর্তে কোনো কম্বো অফার নেই": "No combo offers available right now",
        "শীঘ্রই আসছে। নিয়মিত চেক করুন!": "Coming soon. Check back regularly!",
        "এই কম্বোতে আছে:": "Included in this combo:",
        "আলাদা কিনলে": "Bought separately",
        "আলাদাভাবে কিনলে": "Bought separately",
        "আপনি বাঁচাচ্ছেন": "You are saving",
        "কম্বো মূল্য": "Combo Price",
        "বিস্তারিত": "Details",
        "পুরো কম্বো কার্টে যোগ করুন": "Add Entire Combo to Cart",
        "কার্টে যোগ করতে লগইন করুন": "Login to Add to Cart",
        "সব পণ্য একসাথে কার্টে যোগ হবে": "All items will be added to cart together",
        "অফার শেষ হতে বাকি": "Time left for offer",
        "দিন": "Days",
        "ঘণ্টা": "Hours",
        "মিনিট": "Minutes",
        "সেকেন্ড": "Seconds",

        // Cart & Checkout
        "আপনার কার্ট খালি": "Your cart is empty",
        "কেনাকাটা শুরু করুন": "Start Shopping",
        "অর্ডার সারসংক্ষেপ": "Order Summary",
        "সাবটোটাল": "Subtotal",
        "ডেলিভারি চার্জ": "Delivery Charge",
        "মোট": "Total",
        "চেকআউট করুন": "Proceed to Checkout",
        "কেনাকাটা চালিয়ে যান": "Continue Shopping",
        "সরিয়ে দিন?": "Remove item?",
        "চেকআউট": "Checkout",
        "ডেলিভারি ঠিকানা": "Delivery Address",
        "আপনার সম্পূর্ণ ঠিকানা লিখুন": "Enter your full address",
        "ফোন নম্বর": "Phone Number",
        "ডেলিভারি এরিয়া": "Delivery Area",
        "ঢাকার ভিতর": "Inside Dhaka (৳60)",
        "ঢাকার বাইরে": "Outside Dhaka (৳120)",
        "পেমেন্ট মাধ্যম": "Payment Method",
        "ক্যাশ অন ডেলিভারি (COD)": "Cash on Delivery (COD)",
        "অনলাইন পেমেন্ট (বিকাশ / নগদ / কার্ড)": "Online Payment (bKash / Nagad / Card)",
        "বিশেষ নোট (ঐচ্ছিক)": "Special Notes (Optional)",
        "অর্ডার কনফার্ম করুন": "Confirm Order",

        // Wishlist
        "উইশলিস্ট খালি": "Wishlist is empty",
        "পছন্দের পণ্যগুলো উইশলিস্টে সেভ করুন।": "Save your favorite products to wishlist.",
        "প্রোডাক্ট দেখুন": "Browse Products",
        "উইশলিস্টে আছে": "In Wishlist",
        "উইশলিস্টে যোগ করুন": "Add to Wishlist",
        "উইশলিস্ট থেকে সরাবেন?": "Remove from Wishlist?",

        // Account & Auth
        "ইমেইল": "Email",
        "পাসওয়ার্ড": "Password",
        "পাসওয়ার্ড নিশ্চিত করুন": "Confirm Password",
        "নাম": "Full Name",
        "আপনার নাম লিখুন": "Enter your name",
        "আপনার ইমেইল লিখুন": "Enter your email",
        "বর্তমান পাসওয়ার্ড": "Current Password",
        "নতুন পাসওয়ার্ড": "New Password",
        "সংরক্ষণ করুন": "Save",
        "পরিবর্তন সংরক্ষণ করুন": "Save Changes",
        "বাতিল": "Cancel",
        "নতুন যোগ করুন": "Add New",
        "মুছুন": "Delete",
        "মুছে ফেলুন": "Delete",
        "সম্পাদনা": "Edit",
        "স্ট্যাটাস": "Status",
        "তারিখ": "Date",
        "অ্যাকশন": "Action",
        "ছবি": "Image",
        "মূল্য": "Price",
        "স্টক": "Stock",
        "অর্ডার নং": "Order #",
        "গ্রাহক": "Customer",
        "ফোন": "Phone",
        "পদ্ধতি": "Method",
        "অর্ডার স্ট্যাটাস": "Order Status",
        "নতুন স্ট্যাটাস": "New Status",
        "স্ট্যাটাস আপডেট করুন": "Update Status"
    };

    // Build reverse dictionary (EN -> BN)
    var reverseDict = {};
    for (var bn in dictionary) {
        if (dictionary.hasOwnProperty(bn)) {
            reverseDict[dictionary[bn]] = bn;
        }
    }

    // Sort keys by descending length to match multi-word phrases first
    var bnKeys = Object.keys(dictionary).sort(function (a, b) { return b.length - a.length; });
    var enKeys = Object.keys(reverseDict).sort(function (a, b) { return b.length - a.length; });

    function translateText(text, targetLang) {
        if (!text || typeof text !== 'string') return text;
        var trimmed = text.trim();
        if (!trimmed) return text;

        if (targetLang === 'en') {
            if (dictionary[trimmed]) {
                return text.replace(trimmed, dictionary[trimmed]);
            }
            var res = text;
            for (var i = 0; i < bnKeys.length; i++) {
                var k = bnKeys[i];
                if (res.indexOf(k) !== -1) {
                    res = res.split(k).join(dictionary[k]);
                }
            }
            return res;
        } else {
            if (reverseDict[trimmed]) {
                return text.replace(trimmed, reverseDict[trimmed]);
            }
            var res2 = text;
            for (var j = 0; j < enKeys.length; j++) {
                var ek = enKeys[j];
                if (res2.indexOf(ek) !== -1) {
                    res2 = res2.split(ek).join(reverseDict[ek]);
                }
            }
            return res2;
        }
    }

    function applyLanguage(targetLang) {
        var walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode: function (node) {
                    if (!node.parentElement) return NodeFilter.FILTER_REJECT;
                    var tag = node.parentElement.tagName.toLowerCase();
                    if (tag === 'script' || tag === 'style' || tag === 'textarea') {
                        return NodeFilter.FILTER_REJECT;
                    }
                    if (node.parentElement.classList.contains('lang-toggle-text')) {
                        return NodeFilter.FILTER_REJECT;
                    }
                    return NodeFilter.FILTER_ACCEPT;
                }
            },
            false
        );

        var node;
        while ((node = walker.nextNode())) {
            var original = node.nodeValue;
            var translated = translateText(original, targetLang);
            if (translated !== original) {
                node.nodeValue = translated;
            }
        }

        // Placeholders and Titles
        var inputs = document.querySelectorAll('input[placeholder], textarea[placeholder]');
        inputs.forEach(function (inp) {
            var ph = inp.getAttribute('placeholder');
            if (ph) {
                var newPh = translateText(ph, targetLang);
                if (newPh !== ph) inp.setAttribute('placeholder', newPh);
            }
        });

        // Update Toggle Buttons
        var btns = document.querySelectorAll('.lang-toggle-text');
        btns.forEach(function (btn) {
            if (targetLang === 'en') {
                btn.innerHTML = '🇧🇩 বাংলা';
            } else {
                btn.innerHTML = '🇬🇧 English';
            }
        });

        localStorage.setItem('app_language', targetLang);
    }

    window.toggleSiteLanguage = function () {
        var currentLang = localStorage.getItem('app_language') || 'bn';
        var newLang = currentLang === 'en' ? 'bn' : 'en';
        applyLanguage(newLang);
    };

    document.addEventListener('DOMContentLoaded', function () {
        var savedLang = localStorage.getItem('app_language') || 'bn';
        if (savedLang === 'en') {
            applyLanguage('en');
        } else {
            var btns = document.querySelectorAll('.lang-toggle-text');
            btns.forEach(function (btn) {
                btn.innerHTML = '🇬🇧 English';
            });
        }
    });
})();

