Ext.define('WEBAPP.store.UrInidGrp', {
    //extend: 'Ext.data.Store',
    extend: 'WEBAPP.data.RemoteStore',
    model: 'WEBAPP.model.UrInidGrp',
    pageSize: 20,
    autoLoad: false,
    remoteSort: true,
    sorters: [{ property: 'GRP_NO', direction: 'ASC' }],
    proxy: {
        //type: 'ajax',
        //actionMethods: {
        //    read: 'POST' // by default GET
        //},
        url: '/api/AA0041/T2All'//,
        //reader: {
        //    type: 'json',
        //    rootProperty: 'etts'
        //}
    }
});