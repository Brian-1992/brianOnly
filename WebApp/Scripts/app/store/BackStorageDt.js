Ext.define('WEBAPP.store.BackStorageDt', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BackStorageDt',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'APPTIME', direction: 'ASC' }, { property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0066/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});