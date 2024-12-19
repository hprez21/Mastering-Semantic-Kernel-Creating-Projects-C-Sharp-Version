var builder = new PizzaBuilder()
    .SetDough("Classic")
    .SetSauce("Tomato")
    .SetToppings("Cheese and pepperoni")
    .Build();

Console.WriteLine(builder);


// Final product
public class Pizza
{
    public string Dough { get; set; }
    public string Sauce { get; set; }
    public string Toppings { get; set; }

    public override string ToString()
    {
        return $"Pizza with dough: {Dough}, sauce: {Sauce}, toppings: {Toppings}";
    }
}

public class PizzaBuilder
{
    private Pizza _pizza = new Pizza();

    public PizzaBuilder SetDough(string dough)
    {
        _pizza.Dough = dough;
        return this;
    }

    public PizzaBuilder SetSauce(string sauce)
    {
        _pizza.Sauce = sauce;
        return this;
    }

    public PizzaBuilder SetToppings(string toppings)
    {
        _pizza.Toppings = toppings;
        return this;
    }

    public Pizza Build()
    {
        return _pizza;
    }
}

// Director
//public class PizzaChef
//{
//    private readonly IPizzaBuilder _builder;

//    public PizzaChef(IPizzaBuilder builder)
//    {
//        _builder = builder;
//    }

//    public void BuildClassicPizza()
//    {
//        _builder.SetDough("Classic");
//        _builder.SetSauce("Tomato");
//        _builder.SetToppings("Cheese and pepperoni");
//    }

//    public Pizza GetPizza()
//    {
//        return _builder.Build();
//    }
//}