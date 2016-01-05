type Product = {
        Id: int;
        Name: string;
        Articles: Article list //list of articles related to product
    }

and Article = {
        Product: Product //product this article is linked to
        Id: int;
        Name: string;
        Price: decimal;
    }