Ext.define('WEBAPP.model.MI_MATCLASS', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'MAT_CLSNAME', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});