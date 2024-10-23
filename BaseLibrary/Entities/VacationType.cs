namespace BaseLibrary.Entities
{
    public class VacationType : BaseEntity
    {
        //Many to One relationship with Vacation
        public List<Vacation>? Vacations { get; set; }
    }
}
