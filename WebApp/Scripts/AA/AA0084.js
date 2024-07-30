Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '../../../api/AA0084/SetBcItmanagers'; // 新增/修改/刪除
    var T1Name = "調帳明細表";

    var reportUrl = '/Report/A/AA0084.aspx';

    var T1Rec = 0;
    var T1LastRec = null;

    var Wh_noComboGet = '../../../api/AA0084/GetWh_noCombo';

    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setWh_noComboData() {
        Ext.Ajax.request({
            url: Wh_noComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        Wh_noQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < wh_no.length; i++) {
                            Wh_noQueryStore.add({ VALUE: wh_no[i].VALUE, TEXT: wh_no[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    };
    setWh_noComboData();

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '查詢日期',
                    name: 'P0',
                    id: 'P0',
                    width: 150,
                    labelWidth: 70,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    width: 100,
                    labelWidth: 20,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    name: 'P2',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 300,
                    padding: '0 4 0 4',
                    store: Wh_noQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢', handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢日期</span> 與 <span style=\'color:red\'>庫房別</span> 為必填');
                            return;
                        }

                        reportAutoGenerate(true);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'hidden',
                    name: 'P2',
                    id: 'P2',
                    submitValue: true
                },
            ]
        }]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        //resizeTabs: true,
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BcItmanager', { // 定義於/Scripts/app/store/BcItmanager.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                if (records.length > 0) {
                    T1Grid.down('#update').setDisabled(false);
                    T1Grid.down('#cancel').setDisabled(false);
                } else {
                    T1Grid.down('#update').setDisabled(true);
                    T1Grid.down('#cancel').setDisabled(true);
                }
            }
        },

    });
    function T1Load(clearMsg) {
        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    };

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印', handler: function () {
                    showReport();
                }
            }]
    });

    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var p0 = getDateString(T1Query.getForm().findField('P0').getValue());   //動態年月
            var p1 = getDateString(T1Query.getForm().findField('P1').getValue());   //車型代碼
            var p2 = T1Query.getForm().findField('P2').getValue();
            var qstring = 'STARTDATE=' + p0 + '&ENDDATE=' + p1 + '&WH_NO=' + p2;
            Ext.getDom('mainContent').src = autoGen ? reportUrl + '?' + qstring : '';
        }
    };

    function getDateString(date) {
        var y = date.getFullYear();
        var m = (date.getMonth() + 1).toString();
        var d = (date.getDate()).toString();
        var mm = m.length > 1 ? m : "0" + m;
        var dd = d.length > 1 ? d : "0" + d;
        return y + "-" + mm + "-" + dd;
    };

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            },
            {
                region: 'center',
                xtype: 'form',
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
            }
        ]
    });

    reportAutoGenerate(false);
    T1Query.getForm().findField('P0').focus();
});