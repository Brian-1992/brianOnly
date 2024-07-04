Ext.define('WEBAPP.model.BcManager', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'MANAGERID', type: 'string' },
        { name: 'MANAGERNM', type: 'string' },
        { name: 'USERID', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'MNAME', type: 'string' },
        { name: 'CNT', type: 'string' },
        { name: 'DISPLAY_WHNO', type: 'string' },
        { name: 'TEXT_WHNO', type: 'string' },
        { name: 'DISPLAY_MANAGERID', type: 'string' },
        { name: 'TEXT_MANAGERID', type: 'string' },
        { name: 'TEXT_MANAGERNM', type: 'string' }
    ]
});