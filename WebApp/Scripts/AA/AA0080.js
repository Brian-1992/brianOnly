Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var reportUrl = '/Report/A/AA0080.aspx';

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        fieldLabel: '庫房代碼',
        padding: '0 4 0 4',
        labelWidth: 60,


        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0080/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    function setPrintType() {
        printType = Ext.getUrlParam('printtype');
        if (printType == "frwh") {  // AA0087 只有轉出選項
        } else {
        }
    }


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
                    xtype: 'monthfield',
                    fieldLabel: '年月份',
                    name: 'YYYYMM',
                    labelWidth: 60,
                    width: 150,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                },
                wh_NoCombo,
                {
                    xtype: 'button',
                    text: '查詢', handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
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
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
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
        //T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    //// toolbar,包含換頁、新增/修改/刪除鈕
    //var T1Tool = Ext.create('Ext.PagingToolbar', {
    //    store: T1Store,
    //    displayInfo: true,
    //    border: false,
    //    plain: true,
    //    buttons: [
    //        {
    //            itemId: 't1print', text: '列印', handler: function () {
    //                if (Ext.getCmp('radio1').getValue() == true) {
    //                    T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio1').inputValue);
    //                } else {
    //                    T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio2').inputValue);
    //                }

    //                showReport();
    //            }
    //        }]
    //});


    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            var WH_NO = T1Query.getForm().findField('WH_NO').getValue();   //動態年月
            var YYYYMM = T1Query.getForm().findField('YYYYMM').rawValue; //車型代碼
            var qstring = 'WH_NO=' + WH_NO + '&YYYYMM=' + YYYYMM ;
            Ext.getDom('mainContent').src = reportUrl + '?' + qstring;
        }
    };

    function getDateString(date) {
        //var y = date.getFullYear();

        //var m = (date.getMonth() + 1).toString();
        //var d = (date.getDate()).toString();
        //var mm = m.length > 1 ? m : "0" + m;
        //var dd = d.length > 1 ? d : "0" + d;
        //return y + "-" + mm + "-" + dd;
    }

    
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
    setPrintType();

    T1Query.getForm().findField('YYYYMM').focus();
});
