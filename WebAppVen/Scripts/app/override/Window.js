Ext.define('MMIS.override.Window', {
    override: 'Ext.window.Window',
    maximize: function (animate) {
        var me = this,
            header = me.header,
            tools = me.tools,
            changed;

        if (!me.maximized) {
            me.expand(false);
            if (!me.hasSavedRestore) {
                me.restoreSize = me.getSize();
                me.restorePos = me.getPosition(true);
            }
            if (header) {
                header.suspendLayouts();
            }
            if (me.maximizable) {
                tools.maximize.hide();
                tools.restore.show();
                changed = true;
            }

            if (me.collapseTool) {
                me.collapseTool.hide();
                changed = true;
            }
            if (header) {
                this.resumeHeaderLayout(changed);
            }

            me.maximized = true;
            me.el.disableShadow();

            if (me.dd) {
                me.dd.disable();
                if (header) {
                    header.removeCls(header.indicateDragCls)
                }
            }
            if (me.resizer) {
                me.resizer.disable();
            }

            me.el.addCls(Ext.baseCSSPrefix + 'window-maximized');
            me.container.addCls(Ext.baseCSSPrefix + 'window-maximized-ct');

            me.syncMonitorWindowResize();
            me.fitContainer(animate = (animate || !!me.animateTarget) ? {
                callback: function () {
                    me.fireEvent('maximize', me);
                }
            } : null);
            if (!animate) {
                me.fireEvent('maximize', me);
            }
        }
        return me;
    }
});
