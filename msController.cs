using System;

namespace MemoryCache_Simulator
{
	//-------------------------------------------------------------------//
	//
	//
	//
	//-------------------------------------------------------------------//
	public class MemoryController
	{
		private int nHit;
		private int nMiss;
		private int nCounter;
		private MemoryCache Cache;
		private AssignmentPolicy APolicy;
		private ReplacementAlgorithm RAlgorithm;

		//-----------------------------------------------------------------//
		// Construtor para um Controlador só com Política de Mapeamento
		// Direto.
		public MemoryController (MemoryCache cache, AP_Directo policy)
		{
			Cache = cache;
			nCounter = nHit = nMiss =  0;
			APolicy = policy;
			RAlgorithm = null;
		}

        //-----------------------------------------------------------------//
        // Construtor para um Controlador que pode conter uma Política
        // de Mapeamentos (Associativo ou Por Conjunto) e algum Algoritmo de 
        // Substituição.
        public MemoryController(MemoryCache cache, AssignmentPolicy policy, ReplacementAlgorithm replace)
		{
			Cache = cache;
			APolicy = policy;
			RAlgorithm = replace;
			nCounter = nHit = nMiss = 0;

			if (APolicy is AP_PorConjunto)
			{
				cache.setTabularRepresentation(true);
				RAlgorithm.setTabularRepresentation(true);
			}
		}

		//-----------------------------------------------------------------//
		// Implementa uma operação de Leitura/Escrita na Memória Cache.
		public void R_W (int page)
		{
			CacheStatus status;
			MemoryAddressCache address;

			// Passo 1: Verificar que a página de memória está na cache.
			nCounter++;
			status = Cache.PageInCache(page);
			switch (status)
			{
				case CacheStatus.Hit:	nHit++; break;
			  case CacheStatus.Miss: nMiss++; break;
			}
			
			// Passo 2: Verificar o estado da operação sobre a memória cache
			if (status == CacheStatus.Miss)
			{
				//if (APolicy.GetTypePolicy() == AssignmentPolicyType.Directo)
				if (APolicy is AP_Directo)
				{
					address = APolicy.Calculate(page);
				}
				else
				{
                    // Passo 3: Verificar o estado da cache, para saber se:
                    //	a) a cache ainda não está cheia --> Se utiliza a política de mapeamento.
                    //	b) a cache está cheia --> Se utiliza o algoritmo de substituição para liberar um registro.
                    if (!Cache.IsMemoryFull())
					{
						// a)	Como a memória cache não está cheia, uso o Mapeamento para determinar a zona de 
						//		memória da cache onde escrever a página passada por parâmetro.
						address = APolicy.Calculate(page);
					}
					else
					{
                        // b)	Como a memória cache está cheia, uso o Algoritmo de Substituição para determinar a 
                        //		zona de memória da cache onde escrever a página passada por parâmetro.
                        address = RAlgorithm.Calculate();
					}
				}

				// Passo 4: Como ocorreu um Miss escrevo na memória cache a página passada por parâmetro.
				Cache.WritePage(address, page);

                //Passo 5:	Como se realizou uma operação de escritura na memória cache, devo atualizar 
                //				o estado da Política de Substituição. SOMENTE SE NÃO SE ESTÁ UTILIZANDO A POLÍTICA
                //				RANDOM.
                if (!(RAlgorithm is RA_RANDOM) && RAlgorithm != null) RAlgorithm.FeedBack(address);
			}
			else
			{
                // Como ocorreu um HIT (operação de leitura), devo atualizar o estado da
                // Política de Substituição. SOMENTE SE NÃO SE ESTÁ UTILIZANDO A POLÍTICA RANDOM.
                address = Cache.GetAddressForPage(page);
				if (!(RAlgorithm is RA_RANDOM) && !(RAlgorithm is RA_FIFO) && RAlgorithm != null) RAlgorithm.FeedBack(address);
			}															  

			// Passo 6: Mostro informação da operação sobre a cache.
			LocalStatistics(page, status);
			Cache.PrintMemoryCache();
		}

		//-----------------------------------------------------------------//
		//
		private void LocalStatistics(int page, CacheStatus status)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("[r] Procurando a página ");
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write("{0} ", page);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("na cache... ");
			Console.ForegroundColor = (status == CacheStatus.Hit) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
			Console.Write("{0}", (status == CacheStatus.Hit) ? "-HIT-\n" : "-MISS-\n");
			Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("{0}", (status == CacheStatus.Hit) ? "    Conteúdo da cache:               " : "[w] Conteúdo da cache após inserção: ");
        }

		//-----------------------------------------------------------------//
		// Mostra o resultado global da simulação.
		public void GlobalStatistics()
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("+-- RESUMO DA SIMULAÇÃO: ---------------------------------+");
			Console.WriteLine("|   Fração de acertos às referências de memória: {0:P}   |", (double) nHit / nCounter);
			Console.WriteLine("|   Fração de falhas às referências de memória : {0:P}   |", (double) nMiss / nCounter);
			Console.WriteLine("+---------------------------------------------------------+");
		}
	}
}
