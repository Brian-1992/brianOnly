Ext.define('WEBAPP.model.BC_WHPICKLOT', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'PICK_DATE', type: 'date' },
        { name: 'LOT_NO', type: 'string' },
        { name: 'PICK_USERID', type: 'string' },
        { name: 'PICK_STATUS', type: 'string' },

        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }

    ]
});