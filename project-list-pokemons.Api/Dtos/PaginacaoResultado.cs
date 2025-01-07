namespace project_list_pokemons.Api.Dtos
{
    public class PaginacaoResultado<T>
    {
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Itens { get; set; }
    }

}
