Ext.define('WEBAPP.model.UrInidGrp', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'GRP_NO', type: 'string' },
        { name: 'GRP_NAME', type: 'string' },
        { name: 'CREATE_DATE', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});