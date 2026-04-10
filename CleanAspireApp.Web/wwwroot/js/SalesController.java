@RestController
@RequestMapping("/api/sales")
public class SalesController
{

    @GetMapping
    public List<Sale> getSales()
    {
        return List.of(
            new Sale(1, 100),
            new Sale(2, 250)
        );
    }
}