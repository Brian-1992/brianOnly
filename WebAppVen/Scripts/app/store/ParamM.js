Ext.define('WEBAPP.store.ParamM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ParamM',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'GRP_CODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/GB0006/QueryM',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});