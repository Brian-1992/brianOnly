Ext.define('WEBAPP.store.UrInid', {
    //extend: 'Ext.data.Store',
    extend: 'WEBAPP.data.RemoteStore',
    model: 'WEBAPP.model.UrInid',
    pageSize: 20,
    autoLoad: false,
    remoteSort: true,
    sorters: [{ property: 'INID', direction: 'ASC' }],
    proxy: {
        //type: 'ajax',
        //actionMethods: {
        //    read: 'POST' // by default GET
        //},
        url: '/api/AA0041/All'//,
        //reader: {
        //    type: 'json',
        //    rootProperty: 'etts'
        //}
    }
});