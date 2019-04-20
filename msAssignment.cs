using System;

namespace MemoryCache_Simulator
{
	//-------------------------------------------------------------------//
	//
	//	Classe Abstrata Pai que representa uma Política de Mapeamento.
	//
	//-------------------------------------------------------------------//
	public abstract class AssignmentPolicy
	{
		protected int CacheSize;
		protected bool tabularMemory;

		public abstract MemoryAddressCache Calculate(int page);

		//-----------------------------------------------------------------//
		// Constructor de clase base.
		public AssignmentPolicy(int cache_size)
		{
			CacheSize = cache_size;
		}

		//-----------------------------------------------------------------//
		// Método Virtual. Estabelece se a representação da memória 
		// cache se assume de forma tabular ou não. False: indica representação 
		// linear, True: uma representação tabular (tabela).
		public virtual void setTabularRepresentation(bool tabular)
		{
			tabularMemory = tabular;
			CacheSize = (tabularMemory) ? CacheSize / 2 : CacheSize;
		}
	}

	//-------------------------------------------------------------------//
	//
	// Classe Concreta que representa uma Política de Mapeamento DIRETO.
	//
	//-------------------------------------------------------------------//
	public class AP_Directo : AssignmentPolicy
	{
		//-----------------------------------------------------------------//
		//Construtor da clase.
		public AP_Directo	(int cache_size) : base(cache_size)	{	}

		//-----------------------------------------------------------------//
		// Calcula o mapeamento para a Política: Direta.
		public override MemoryAddressCache Calculate (int page)
		{
			MemoryAddressCache address;

			// A política de mapeamento DIRETO define-se pela expressão: D MOD E,
			// onde D é o valor do endereço de memória e E é o tamanho da memória
			// cache.
			address.Row = page % CacheSize;
			address.Col = 0;

			return address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito.
		public override void setTabularRepresentation(bool tabular)
		{
			throw new NotImplementedException("A política de mapeamento DIRECTA não tem represetação tabular.!!!");
		}
	}

	//-------------------------------------------------------------------//
	//
	//  Classe Concreta que representa uma Política de Mapeamento ASSOCIATIVO.
	//
	//-------------------------------------------------------------------//
	public class AP_Associativa : AssignmentPolicy
	{
		private int indexMemory;

		//-----------------------------------------------------------------//
		//
		public AP_Associativa (int cache_size) : base(cache_size)
		{
			indexMemory = 0;
		}

		//-----------------------------------------------------------------//
		// Calcula o mapeamento para a Política: Associativa.
		public override MemoryAddressCache Calculate (int page)
		{
			MemoryAddressCache address;

			// A política de mapeamento ASSOCIATIVA define-se como a concessão 
			// consecutiva de endereços de memória.
			address.Row = indexMemory % CacheSize;
			address.Col = 0;
			indexMemory++;

			return address;
		}
	}

	//-------------------------------------------------------------------//
	//
	//  Classe Concreta que representa uma Política de Mapeamento POR-CONJUNTO.
	//
	//-------------------------------------------------------------------//
	public class AP_PorConjunto : AssignmentPolicy
	{
		private int []colIndex;

		//-----------------------------------------------------------------//
		// Constructor de clase.
		public AP_PorConjunto (int cache_size) : base(cache_size / 2)
		{
			colIndex = new int[CacheSize];
			for (int i = 0; i < CacheSize; i++) colIndex[i] = 0;
		}

		//-----------------------------------------------------------------//
		// Calcula o mapeamento para a Política: Por Conjuntos.
		public override MemoryAddressCache Calculate (int page)
		{
			MemoryAddressCache address;

			// A política de mapeamento ASSOCIATIVA POR CONJUNTOS usa as políticas anteriores
			// descrição: Usa mapeamento DIRETO para selecionar a linha, e 
			//						CONSECUTIVO para selecionar a columna.
			AP_Directo directo = new AP_Directo(CacheSize);
			address.Row = directo.Calculate(page).Row;
			address.Col = colIndex[address.Row] % 2;
			colIndex[address.Row]++;

			return address;
		}

		//-----------------------------------------------------------------//
		// Método Virtual Sobrescrito.
		public override void setTabularRepresentation (bool tabular)
		{
			throw new NotImplementedException("A política de mapeamento POR-CONJUNTO é implicitamente tabular!!!");
		}
	}
}
