namespace NetCoreLedger.Business
{
    public class Ledger
    {
        private Chain _chain;
        private Store _store; 
        
        public Ledger()
        {
            // init ledger
            // Try load from disk and synchronize
            _chain = new Chain();
            _store = new Store("data");

            _store.SyncChain(_chain);
        }
    }
}
