using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Data
{
    /// <summary>
    /// Provides the default 240+ item restaurant menu used to seed a fresh database.
    /// </summary>
    public static class SeedData
    {
        public static List<MenuItem> GetMenuItems()
        {
            var items = new List<MenuItem>();

            AddCategory(items, "Starters", 250, new[]
            {
                "Chicken Spring Rolls","Vegetable Spring Rolls","Chicken Wings (Spicy)","Chicken Wings (BBQ)",
                "Fish Fingers","Prawn Tempura","Chicken Nuggets","Onion Rings","Cheese Sticks","Chicken Samosa",
                "Vegetable Samosa","Chicken Pakora","Aloo Pakora","Dahi Baray","Chicken Malai Boti Skewers",
                "Reshmi Kebab","Beef Seekh Kebab","Chapli Kebab","Shami Kebab","Fish Pakora","Chicken 65",
                "Crispy Chicken Popcorn","Nachos with Cheese","Loaded Nachos","Chicken Tikka Bites"
            });

            AddCategory(items, "Soups", 200, new[]
            {
                "Chicken Corn Soup","Hot & Sour Soup","Thai Soup","Cream of Mushroom Soup","Tomato Soup",
                "Chicken Manchow Soup","Vegetable Soup","Lentil Soup","Chicken Noodle Soup","Seafood Soup"
            });

            AddCategory(items, "Salads", 250, new[]
            {
                "Green Salad","Caesar Salad","Chicken Caesar Salad","Greek Salad","Fruit Salad",
                "Kachumbar Salad","Coleslaw","Russian Salad","Chickpea Salad","Garden Salad"
            });

            AddCategory(items, "BBQ & Tikka", 350, new[]
            {
                "Chicken Tikka","Chicken Malai Tikka","Mutton Tikka","Beef Tikka","Chicken Boti","Mutton Boti",
                "Behari Kebab","Chicken Seekh Kebab","Mutton Seekh Kebab","Tandoori Chicken (Half)",
                "Tandoori Chicken (Full)","Fish Tikka","Prawn BBQ","Chicken Wings BBQ","Mixed Grill Platter",
                "Peshawari Tikka","Bihari Boti","Lahori Charga","Achari Tikka","Garlic Chicken Tikka"
            });

            AddCategory(items, "Chicken Curries", 400, new[]
            {
                "Chicken Karahi","Chicken White Karahi","Butter Chicken","Chicken Handi","Chicken Qorma",
                "Chicken Jalfrezi","Chicken Achari","Chicken Do Pyaza","Chicken Saag","Chicken Palak",
                "Chicken Bhuna","Chicken Kadai","Chicken Rara","Chicken Peshawari","Chicken Makhani"
            });

            AddCategory(items, "Mutton & Beef Curries", 500, new[]
            {
                "Mutton Karahi","Beef Karahi","Mutton Qorma","Beef Qorma","Mutton Handi","Beef Nihari",
                "Mutton Nihari","Beef Kunna","Mutton Kunna","Beef Rara","Mutton Rara","Paya",
                "Beef Kofta Curry","Mutton Kofta Curry","Beef Bhuna"
            });

            AddCategory(items, "Vegetarian & Daal", 250, new[]
            {
                "Daal Makhani","Daal Chana","Daal Mash","Daal Tarka","Mixed Vegetable Curry","Aloo Gobi",
                "Bhindi Masala","Palak Paneer","Chana Masala","Aloo Palak","Baingan Bharta","Karela",
                "Aloo Methi","Shahi Paneer","Paneer Tikka Masala"
            });

            AddCategory(items, "Chinese", 350, new[]
            {
                "Chicken Manchurian","Vegetable Manchurian","Chicken Chowmein","Vegetable Chowmein",
                "Chicken Fried Rice","Vegetable Fried Rice","Egg Fried Rice","Chicken Schezwan",
                "Prawns in Garlic Sauce","Sweet & Sour Chicken","Chilli Chicken","Chicken in Black Bean Sauce",
                "Beef Chowmein","Chicken Cashew Nut","Kung Pao Chicken","Shrimp Fried Rice",
                "Chicken Hot Garlic","Vegetable Spring Chowmein","Thai Chicken","Chicken Singapore Rice"
            });

            AddCategory(items, "Fast Food", 300, new[]
            {
                "Zinger Burger","Chicken Burger","Beef Burger","Cheese Burger","Club Sandwich",
                "Chicken Sandwich","Grilled Chicken Wrap","Chicken Shawarma","Beef Shawarma","Chicken Roll",
                "Beef Roll","Fries (Regular)","Fries (Peri Peri)","Fish Burger","Crispy Chicken Burger",
                "Chicken Fajita Wrap","Hot Dog","Chicken Panini","Loaded Fries","Chicken Quesadilla"
            });

            AddCategory(items, "Pizza", 700, new[]
            {
                "Chicken Tikka Pizza","Chicken Fajita Pizza","Margherita Pizza","Vegetable Pizza",
                "BBQ Chicken Pizza","Beef Pepperoni Pizza","Cheese Lovers Pizza","Peri Peri Chicken Pizza",
                "Chicken Supreme Pizza","Hawaiian Pizza","Four Seasons Pizza","Spicy Chicken Pizza",
                "Malai Boti Pizza","Seafood Pizza","Vegetarian Supreme Pizza"
            });

            AddCategory(items, "Pasta", 450, new[]
            {
                "Chicken Alfredo Pasta","Chicken Tikka Pasta","White Sauce Pasta","Red Sauce Pasta",
                "Pink Sauce Pasta","Beef Lasagna","Chicken Lasagna","Mac and Cheese","Penne Arrabiata",
                "Spaghetti Bolognese"
            });

            AddCategory(items, "Rice & Biryani", 350, new[]
            {
                "Chicken Biryani","Mutton Biryani","Beef Biryani","Vegetable Biryani","Chicken Pulao",
                "Mutton Pulao","Beef Pulao","Sindhi Biryani","Bombay Biryani","Kabuli Pulao",
                "Chicken Tikka Biryani","Fish Biryani","Prawn Biryani","Zarda","Khichdi"
            });

            AddCategory(items, "Breads", 60, new[]
            {
                "Plain Naan","Garlic Naan","Butter Naan","Cheese Naan","Roghni Naan","Tandoori Roti",
                "Kulcha","Paratha","Aloo Paratha","Sheermal"
            });

            AddCategory(items, "Desserts", 200, new[]
            {
                "Gulab Jamun","Ras Malai","Kheer","Shahi Tukda","Kulfi Falooda","Chocolate Lava Cake",
                "New York Cheesecake","Brownie with Ice Cream","Ice Cream Sundae","Fruit Trifle",
                "Tiramisu","Red Velvet Cake","Gajar Halwa","Custard","Jalebi"
            });

            AddCategory(items, "Hot Beverages", 100, new[]
            {
                "Tea","Green Tea","Kashmiri Chai","Coffee","Cappuccino","Espresso","Latte",
                "Hot Chocolate","Doodh Patti","Black Coffee"
            });

            AddCategory(items, "Cold Beverages & Juices", 120, new[]
            {
                "Soft Drink (Coke)","Soft Drink (Pepsi)","Sprite","Fresh Orange Juice","Mango Juice",
                "Apple Juice","Lemonade","Mint Margarita","Lassi (Sweet)","Lassi (Salted)","Mango Lassi",
                "Iced Tea","Chocolate Milkshake","Vanilla Milkshake","Mineral Water"
            });

            return items; // 240 items total
        }

        private static void AddCategory(List<MenuItem> items, string category, decimal basePrice, string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                // Gives a little natural price variation within each category instead of one flat price.
                decimal price = basePrice + (i % 6) * 25;
                items.Add(new MenuItem { Name = names[i], Category = category, Price = price });
            }
        }
    }
}
