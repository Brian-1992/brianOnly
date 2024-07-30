Ext.define('WEBAPP.store.MiUnitcodeAA0119', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MiUnitcode',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'UNIT_CODE', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0119/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});