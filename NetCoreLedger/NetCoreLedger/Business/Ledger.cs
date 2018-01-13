using NetCoreLedger.Domain;

namespace NetCoreLedger.Business
{
    public class Ledger
    {
        private Chain _chain;
        private Store _store; 
        
        public Ledger()
        {

        }

        public void Initialize()
        {
            // init ledger
            // Try load from disk and synchronize
            // If we can't load set initial and continue

            _chain = new Chain();
            _store = new Store("data");

            _store.SyncChain(_chain);

            if (_chain.Count == 0)
            {
                var genesis = Block.GenerateGenesis();
                _chain.AddLast(genesis.Header);
                _store.Append(genesis);
            }
        }

        public void AddBlock(Block block)
        {
            // Validate chain
            // Add to chain
            // Add to store
            _chain.Validate();

        }

        private void Validate()
        {
        }
    }
}
