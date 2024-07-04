Ext.define('WEBAPP.store.ParamD', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ParamD',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'DATA_SEQ', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/GB0006/QueryD',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});