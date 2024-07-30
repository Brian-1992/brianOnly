Ext.define('WEBAPP.store.PhMailspD', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PhMailspD',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'agen_no', direction: 'ASC' }, { property: 'M_CONTID', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BD0003/DetailAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});