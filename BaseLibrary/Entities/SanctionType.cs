namespace BaseLibrary.Entities
{
    public class SanctionType : BaseEntity
    {
        //Many to One relationship with Vacation
        public List<Sanction>? Sanctions { get; set; }
    }
}