Ext.define('WEBAPP.store.ME_MDFD', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ME_MDFD',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0041/DetailAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});