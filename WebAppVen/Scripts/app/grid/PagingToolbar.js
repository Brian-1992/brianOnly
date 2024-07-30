Ext.define('MMIS.grid.PagingToolbar', {
    extend: 'Ext.toolbar.Paging',

	displayInfo: true,
	border: true,

    initComponent: function () {
        this.callParent(arguments);
        this.remove(this.getComponent(this.items.length -2));
    }

});