Ext.define('WEBAPP.model.PHRSDPT', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'RXTYPE', type: 'string' },
        { name: 'RXDATEKIND', type: 'string' },
        { name: 'DEADLINETIME', type: 'string' },
        { name: 'WORKFLAG', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' } 
    ]
});