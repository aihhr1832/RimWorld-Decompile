namespace RimWorld.BaseGen
{
	public class SymbolResolver_DiningRoom : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("doors", rp);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect.ContractedBy(1);
			BaseGen.symbolStack.Push("interior_diningRoom", resolveParams);
			BaseGen.symbolStack.Push("emptyRoom", rp);
		}
	}
}
