Ext.define('WEBAPP.store.MiUnitcode', {
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
        url: '/api/AA0120/GetMiUnitcode',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});