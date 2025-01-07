using System.Text.RegularExpressions;

namespace project_list_pokemons.Api.Utils
{
    public static class CpfValidator
    {
        /// <summary>
        /// Verifica se um CPF é válido.
        /// </summary>
        /// <param name="cpf">O CPF a ser validado.</param>
        /// <returns>True se o CPF for válido, False caso contrário.</returns>
        public static bool IsValid(string cpf)
        {
            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, "[^0-9]", "");

            // Verifica se o CPF possui exatamente 11 dígitos
            if (cpf.Length != 11 || new string(cpf[0], 11) == cpf)
                return false;

            // Multiplicadores para os cálculos dos dígitos verificadores
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            // Calcula o primeiro dígito verificador
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }
            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            // Verifica o primeiro dígito
            tempCpf += primeiroDigito;
            soma = 0;

            // Calcula o segundo dígito verificador
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }
            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            // Verifica o segundo dígito
            return cpf.EndsWith($"{primeiroDigito}{segundoDigito}");
        }
    }
}
