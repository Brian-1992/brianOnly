Ext.define('WEBAPP.store.PhSmallD', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.PhSmallD',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'SEQ', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/BC0002/DetailAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});