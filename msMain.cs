using System;
using System.IO;
using System.Collections.Generic;

namespace MemoryCache_Simulator
{
    class Program
    {
        static private int initMemory;
        static private SettingApp config;
        static private List<int> ramPages;

        //-----------------------------------------------------------------------------//
        // Função principal.
        //-----------------------------------------------------------------------------//
        static void Main(string[] args)
        {
            // Se carrega desde arquivo a configuração do simulador.
            config = new SettingApp();
            if (!LoadSetting(ref config)) {
                Console.Read();
                return;
            }

            // Garanto que o tamanho da memória cache sempre seja base 2.
            config.CacheSize = FixMemoryTo2Potency();

            try {
                // Se carrega desde arquivo os endereços de memória principal.
                ramPages = readFile_PagesRAM(config.RamPagesFile);
            }
            catch (FileNotFoundException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[!] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Não foi possível carregar o arquivo '");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(config.RamPagesFile);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("'. Verifique e tente novamente!!!\n\n");
                return;
            }

            // Loop principal da aplicação.
            char option;
            do {
                Console.Clear();
                PrintWelcome();
                PrintConfiguration();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n» Selecione o tipo de simulação:");
                Console.WriteLine("   (1) Execução simples: 1 mapeamento com 1 substituição (exceto se mapeamento é Direto)");
                Console.WriteLine("   (2) Execução por grupo: 1 mapeamento com M substituições (exceto se mapeamento é Direto)");
                Console.WriteLine("   (3) Execução múltiple: N mapeamentos com M substituições");
                Console.WriteLine("   (0) Sair.");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Opção: ");
                option = Convert.ToChar(Console.Read());

                switch (option) {
                    case '1': SingleSimulation(); break;
                    case '2': GroupSimulation(); break;
                    case '3': MultipleSimulation(); break;
                    default: continue;
                }

                Console.WriteLine("\nDigite qualquer tecla para ir ao menu de seleção...");
                Console.ReadKey();
                FlushKeyboard();
            } while (option != '0');
        }

        //-----------------------------------------------------------------------------//
        // Executa a simulação de um mapeamento	com um algoritmo de substituição
        // de acordo com os dados carregados desde arquivo "setting.ini"
        static void SingleSimulation()
        {
            ExecutionOutput(config.Mapping, config.Replace);
        }

        //-----------------------------------------------------------------------------//
        // Executa as simulações de um tipo de mapeamento com todos os algoritmos de 
        // substituição.
        static void GroupSimulation()
        {
            if (config.Mapping == AssignmentPolicyType.Direto) {
                ExecutionOutput(config.Mapping, config.Replace);
            }
            else {
                foreach (ReplacementAlgorithmType alg in Enum.GetValues(typeof(ReplacementAlgorithmType))) {
                    ExecutionOutput(config.Mapping, alg);
                    WaitKey();
                }
            }
        }

        //-----------------------------------------------------------------------------//
        // Executa as simulações de todas as políticas de mapeamentos e todos os
        // algoritmos de substituição.
        static void MultipleSimulation()
        {
            foreach (AssignmentPolicyType map in Enum.GetValues(typeof(AssignmentPolicyType))) {
                if (map == AssignmentPolicyType.Direto) {
                    ExecutionOutput(config.Mapping, config.Replace);
                    WaitKey();
                }
                else {
                    foreach (ReplacementAlgorithmType alg in Enum.GetValues(typeof(ReplacementAlgorithmType))) {
                        ExecutionOutput(map, alg);
                        WaitKey();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------//
        // Constrói um controlador de memória segundo a configuração carregada desde 
        // o arquivo <setting.ini>
        static MemoryController BuildMemoryController(int size, AssignmentPolicyType mapping, ReplacementAlgorithmType replacing = 0)
        {
            MemoryCache memory = new MemoryCache(size);
            AssignmentPolicy assignment = null;
            ReplacementAlgorithm replace = null;

            if (mapping == AssignmentPolicyType.Direto)
                return new MemoryController(memory, new AP_Directo(size));
            else {
                // Dependendo da configuração, crio a Política de Mapeamento necessária. 
                switch (mapping) {
                    case AssignmentPolicyType.Associativo: assignment = new AP_Associativa(size); break;
                    case AssignmentPolicyType.PorConjunto: assignment = new AP_PorConjunto(size); break;
                }

                // Dependendo da configuração, crio o Algoritmo de Substituição necessário.
                switch (replacing) {
                    case ReplacementAlgorithmType.Fifo: replace = new RA_FIFO(size); break;
                    case ReplacementAlgorithmType.Lfu: replace = new RA_LFU(size); break;
                    case ReplacementAlgorithmType.Lru: replace = new RA_LRU(size); break;
                    case ReplacementAlgorithmType.Mru: replace = new RA_MRU(size); break;
                    case ReplacementAlgorithmType.Random: replace = new RA_RANDOM(size); break;
                }
            }

            return new MemoryController(memory, assignment, replace);
        }

        //-----------------------------------------------------------------------------//
        // Ler os dados dos endereços de memória de um arquivo cujo nome é 
        // passado por parâmetro.
        static List<int> readFile_PagesRAM(string fname)
        {
            string line;
            List<int> pages = new List<int>();
            StreamReader file = new StreamReader(fname);

            while ((line = file.ReadLine()) != null)
                pages.Add(Convert.ToInt32(line));

            file.Close();
            return pages;
        }

        //-----------------------------------------------------------------------------//
        // Carrega os dados de configuração do Simulador.
        static bool LoadSetting(ref SettingApp s)
        {
            bool flag;
            string line, param, value;
            string[] tokens;
            param = "";
            flag = true;

            try {
                StreamReader file = new StreamReader("setting.ini");
                while ((line = file.ReadLine()) != null) {
                    if (line.StartsWith("#"))
                        continue;

                    tokens = line.Split(':');
                    if (tokens != null && tokens.Length == 2) {
                        param = tokens[0].Trim(' ', '\t');
                        switch (param) {
                            case "CACHE_SIZE": s.CacheSize = Convert.ToInt32(tokens[1]); break;
                            case "RAM_PAGES_FILE": s.RamPagesFile = tokens[1].Trim(' ', '\t'); break;
                            case "ASSIGNMENT_POLICY":
                                value = tokens[1].Trim(' ', '\t');

                                switch (value) {
                                    case "DIRETO": s.Mapping = AssignmentPolicyType.Direto; break;
                                    case "ASSOCIATIVO": s.Mapping = AssignmentPolicyType.Associativo; break;
                                    case "POR-CONJUNTO": s.Mapping = AssignmentPolicyType.PorConjunto; break;
                                    default:
                                        PrintValidValues("ASSIGNMENT_POLICY", value, 1);
                                        flag = false;
                                        break;
                                }
                                break;
                            case "REPLACEMENT_ALGORITHM":
                                value = tokens[1].Trim(' ', '\t');

                                switch (value) {
                                    case "FIFO": s.Replace = ReplacementAlgorithmType.Fifo; break;
                                    case "LRU": s.Replace = ReplacementAlgorithmType.Lru; break;
                                    case "LFU": s.Replace = ReplacementAlgorithmType.Lfu; break;
                                    case "MRU": s.Replace = ReplacementAlgorithmType.Mru; break;
                                    case "RANDOM": s.Replace = ReplacementAlgorithmType.Random; break;
                                    default:
                                        PrintValidValues("REPLACEMENT_ALGORITHM", value, 1);
                                        flag = false;
                                        break;
                                }
                                break;
                            default:
                                PrintValidValues(param, "", 2);
                                flag = false;
                                break;
                        }
                    }
                }
            }
            catch (FileNotFoundException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[!] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Não foi possível carregar o arquivo '");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("setting.ini");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("'. Verifique e tente novamente!!!\n\n");
                flag = false;
            }
            catch (FormatException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[!] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Não foi possível obter o valor do parâmetro '");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}", param);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("'. Verifique e tente novamente!!!\n\n");
                flag = false;
            }

            return flag;
        }

        //-----------------------------------------------------------------------------//
        // Imprimir mensagem de boas vindas da aplicação.
        static void PrintWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("**********************************************************\n**");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\tSIMULADOR DE GERENCIAMENTO DE MEMÓRIA CACHE");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\t**\n**\t\t\t\t\t\t\t**\n");
            Console.Write("** Autores:\t\t\t\t\t\t**\n");
            Console.Write("**    » Carlos R. Herrera Márquez\t\t\t**\n");
            Console.Write("**    » Ruslán Guerra Marzo\t\t\t\t**\n**\t\t\t\t\t\t\t**\n");
            Console.Write("**********************************************************\n\n");
        }

        //-----------------------------------------------------------------------------//
        // Imprime a configuração carregada desde arquivo.
        static void PrintConfiguration()
        {
            PrintParamValue("Capacidade da cache: ", config.CacheSize);
            string map = config.Mapping.ToString().ToUpper();

            if (map == "PORCONJUNTO")
                map = "ASSOCIATIVO POR CONJUNTOS";

            PrintParamValue("Esquema de mapeamento: ", map);
            PrintParamValue("Algoritmo de substituição: ", config.Replace.ToString().ToUpper());
            PrintParamValue("Nome do arquivo de entrada: ", config.RamPagesFile);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void PrintParamValue(string paramText, object value)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(paramText);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(value);
        }

        //-----------------------------------------------------------------------------//
        // Limpa o buffer do stream de entrada por teclado.
        private static void FlushKeyboard()
        {
            while (Console.In.Peek() != -1)
                Console.In.Read();
        }

        //-----------------------------------------------------------------------------//
        // Ajusta o valor de memória cache, carregada desde arquivo para que sempre
        // seja base 2 ==> 2^1, 2^2, 2^3 2^4, 2^5, 2^6...
        private static int FixMemoryTo2Potency()
        {
            int pow = 1;
            initMemory = config.CacheSize;

            while (Math.Pow(2, pow) < initMemory)
                pow++;

            return Convert.ToInt32(Math.Pow(2, pow));
        }

        private static void PrintValidValues(string param, string value, int tipo)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[!] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Não foi possível identificar o ");
            Console.Write("{0} '", tipo == 1 ? "valor" : "parâmetro");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0}", tipo == 1 ? value : param);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (tipo == 1)
                Console.Write("' do parâmetro {0}.", param);
            else
                Console.Write("'.");

            Console.Write(" Verifique e tente novamente!!!\n\n");
            Console.Write("» Os parâmetros e valores válidos são:\n");
            Console.Write(" - CACHE_SIZE\n");
            Console.Write(" - RAM_PAGES_FILE\n");
            Console.Write(" - ASSIGNMENT_POLICY: [ DIRETO | ASSOCIATIVO | POR-CONJUNTO ]\n");
            Console.Write(" - REPLACEMENT_ALGORITHM: [ FIFO | LRU | LFU | MRU | RANDOM ]\n\n");
        }

        static void ExecutionOutput(AssignmentPolicyType mapeamento, ReplacementAlgorithmType algoritmo)
        {
            string map = mapeamento.ToString().ToUpper();
            string alg = algoritmo.ToString().ToUpper();
            MemoryController controller;
            //-----
            // Execução: Mapeamento DIRETO || (Mapeamento X & Substituição Y).
            //-----
            if (map == "PORCONJUNTO")
                map = "ASSOCIATIVO POR CONJUNTOS";

            // Passo 1: Crio um controlador de memória cache
            if (mapeamento == AssignmentPolicyType.Direto) {
                PrintHeader(map, "", 1);
                controller = BuildMemoryController(config.CacheSize, mapeamento);
            }
            else {
                PrintHeader(map, alg, 2);
                controller = BuildMemoryController(config.CacheSize, mapeamento, algoritmo);
            }

            // Passo 2: Realizo as operações de leitura/escrita sobre a cache.
            foreach (var page in ramPages)
                controller.R_W(page);

            // Passo 3: Mostro as estatísticas finais da simulação.
            if (mapeamento == AssignmentPolicyType.Direto)
                PrintHeader(map, "", 1);
            else
                PrintHeader(map, alg, 2);

            controller.GlobalStatistics();

        }

        static void PrintHeader(string map, string alg, int tipo)
        {
            int headerSize = map.Length + alg.Length + ((tipo == 1) ? 18 : 35);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            PrintLine(headerSize);
            Console.Write("|   Mapeamento: {0}", map);

            if (tipo == 2)
                Console.Write(" & Substituição: {0}", alg);

            Console.WriteLine("   |");
            PrintLine(headerSize);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void PrintLine(int size)
        {
            Console.Write("+");

            for (int i = 1; i <= size; i++)
                Console.Write("-");

            Console.WriteLine("+");
        }

        static void WaitKey()
        {
            Console.WriteLine("\nPressione uma tecla para executar a próxima simulação...");
            Console.ReadKey();
        }
	}
}