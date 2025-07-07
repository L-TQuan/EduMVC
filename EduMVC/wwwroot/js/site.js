const updateShoppingCartDiv = (shoppingCart) => {
    let divShoppingCart = document.getElementById("shopping-cart");
    if (divShoppingCart) {
        let cartItemCount = shoppingCart.courseIds ? shoppingCart.courseIds.length : 0;
        divShoppingCart.textContent = cartItemCount;
    }
};