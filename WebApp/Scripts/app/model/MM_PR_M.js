Ext.define('WEBAPP.model.MM_PR_M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PR_NO', type: 'string' },
        { name: 'M_STOREID', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'PR_DEPT', type: 'string' },
        { name: 'PR_STATUS', type: 'string' },
        { name: 'PR_TIME', type: 'date' },
        { name: 'PR_USER', type: 'string' },
        { name: 'CREATE_TIME', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' }
     ]
});