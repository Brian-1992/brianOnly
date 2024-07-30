Ext.define('WEBAPP.store.BG0006VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BG0006M',
    pageSize: 50,
    remoteSort: true,
    sorters: [{ property: 'AGEN_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BG0006/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});