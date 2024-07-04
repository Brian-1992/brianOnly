Ext.define('MMIS.override.grid.feature.Summary', {
    override: 'Ext.grid.feature.Summary',
    onStoreUpdate: function () {
        try {
            this.callParent();
        }
        catch (e) {
            if (window.console) {
                console.error(e.message);
            }
        }
    }
});