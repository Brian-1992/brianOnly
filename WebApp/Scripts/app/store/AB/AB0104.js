Ext.define('WEBAPP.store.AB.AB0104', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0104',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' },
        { property: 'APPTIME', direction: 'ASC' },
        { property: 'DOCNO', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0104/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});