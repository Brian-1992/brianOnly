Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Set = '../../../api/AA0082/Set';
    var T1Exp = '../../../api/AA0082/Excel';
    var purtypeSet = '../../../api/AA0082/SetPurtype';
    var mastPurtypeSet = '../../../api/AA0082/SetMastPurtype';
    var chkTempGet = '/api/AA0082/ChkTempData';
    var rpTempGet = '/api/AA0082/RpTempData';
    var T1Name = "依甲案進貨報表";
    
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 95
        },
        border: false,
        items: [
        {
            xtype: 'monthfield',
            fieldLabel: '月份別',
            name: 'P0',
            labelWidth: 60,
            width: 140,
            padding: '0 4 0 4',
            fieldCls: 'required',
            allowBlank: false
        }, {
            xtype: 'displayfield',
            value: '預設前90名為甲案，排序'
        }, {
            xtype: 'radiogroup',
            name: 'P1',
            items: [
                { boxLabel: '日平均消耗量', name: 'P1', id: 'P1_0', inputValue: '0', width: 100, checked: true },
                { boxLabel: '日平均消耗金額', name: 'P1', id: 'P1_1', inputValue: '1', width: 110 }
            ],
            width: 210,
            padding: '0 4 0 4',
        }, {
            xtype: 'hidden',
            name: 'property'
        }, {
            xtype: 'hidden',
            name: 'direction'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: function () {
                if (T1Query.getForm().isValid())
                    chkTempData();
                else
                    Ext.Msg.alert('訊息', '查詢條件為必填');
            }
        }, {
            xtype: 'button',
            text: '預設甲案',
            itemId: 'btnT1',
            disabled: true,
            handler: function () {
                Ext.MessageBox.confirm('預設甲案', '是否要將 前90 筆設定為甲案？', function (btn, text) {
                    if (btn === 'yes') {
                        T1Submit();
                    }
                });
            }
        }, {
            xtype: 'button',
            text: '暫存',
            itemId: 'btnSave',
            disabled: true,
            handler: function () {
                Ext.MessageBox.alert('訊息', '資料儲存成功');
            }
        }, {
            xtype: 'button',
            text: '轉正式',
            itemId: 'btnTran',
            disabled: true,
            handler: function () {
                Ext.MessageBox.confirm('轉正式', '確定要將甲案資料轉正式？', function (btn, text) {
                    if (btn === 'yes') {
                        updateMastPurtype();
                    }
                });
            }
        }, {
            xtype: 'button',
            text: '匯出',
            itemId: 'btnExport',
            disabled: true,
            handler: function () {
                Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                    if (btn === 'yes') {
                        var p = new Array();
                        p.push({ name: 'FN', value: '依甲案進貨報表匯出.xls' }); //檔名
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').originalValue.P1 }); //SQL篩選條件
                        PostForm(T1Exp, p);
                        msglabel('訊息區:匯出完成');
                    }
                });
            }
        }, {
            xtype: 'button',
            text: '列印',
            itemId: 'btnPrint',
            disabled: true,
            handler: function () {
                showReport();
            }
        }]
    });

    function T1Submit() {
        var f = T1Query.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.findField('property').setValue(T1Store.getSorters().items[0]._property);
        f.findField('direction').setValue(T1Store.getSorters().items[0]._direction);
        f.submit({
            url: T1Set,
            success: function (form, action) {
                myMask.hide();
                Ext.Msg.alert('成功','資料處理完成');
                //T1Load();
                T1Store.load({
                    params: {
                        start: 0
                    }
                });
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('失敗', action.result.msg);
                        break;
                }
            }
        });
    }

    var T1Store = Ext.create('WEBAPP.store.AA.AA0082', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    if (records[0].data['SIGN_TIME'] != '' && records[0].data['SIGN_TIME'] != null)
                    {
                        T1Query.down('#btnT1').setDisabled(true);
                        T1Query.down('#btnSave').setDisabled(true);
                        T1Query.down('#btnTran').setDisabled(true);
                    }
                    else
                    {
                        T1Query.down('#btnT1').setDisabled(false);
                        T1Query.down('#btnSave').setDisabled(false);
                        T1Query.down('#btnTran').setDisabled(false);
                    }
                    T1Query.down('#btnExport').setDisabled(false);
                    T1Query.down('#btnPrint').setDisabled(false);
                }
            }
        }
    });
    function T1Load() {
        msglabel('');
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        if (T1Query.getForm().isValid()) {
            // 排序方式恢復為使用ROWITEM並載入
            T1Store.getSorters().removeAll();
            T1Store.getSorters().add('ROWITEM');
        }
        else
            Ext.Msg.alert('訊息', '查詢條件為必填');
    }
    //T1Load();
    
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="/Report/A/AA0082.aspx?p0=' + T1Query.getForm().findField('P0').rawValue + '&p1=' + T1Query.getForm().findField('P1').originalValue.P1
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "項次",
            dataIndex: 'ROWITEM',
            style: 'text-align:left',
            align: 'center',
            // sortable: false,
            width: 45
        }, {
            text: "藥品院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'center',
            // sortable: false,
            width: 90
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            // sortable: false,
            width: 260
        }, {
            text: "日平均消耗量",
            dataIndex: 'DAYAVGQTY',
            style: 'text-align:left',
            align: 'right',
            // sortable: false,
            width: 100
        }, {
            text: "日平均消耗金額",
            dataIndex: 'DAYAMOUNT',
            style: 'text-align:left',
            align: 'right',
            // sortable: false,
            width: 110
        }, {
            text: "月平均消耗量",
            dataIndex: 'MONAVGQTY',
            style: 'text-align:left',
            align: 'right',
            // sortable: false,
            width: 100
        }, {
            text: "月平均消耗金額",
            dataIndex: 'MONAMOUNT',
            style: 'text-align:left',
            align: 'right',
            // sortable: false,
            width: 110
        }, {
            text: '甲案',
            dataIndex: 'PURCHASE1',
            style: 'text-align:left',
            align: 'center',
            sortable: false,
            width: 50,
            renderer: function (val, meta, record) {
                var disabledString = '';
                if (record.data['SIGN_TIME'] != null && record.data['SIGN_TIME'] != '')
                    disabledString = 'disabled';
                if (val == 'Y')
                    return "<input type='radio' checked='checked' " + disabledString + " >";
                else
                    return "<input type='radio' " + disabledString + " >";
            },
            listeners: {
                click: function (grid, c, rowIdx, colIdx, e, record) {
                    if (record.data['PURCHASE1'] != 'Y')
                        updatePurtype(record.data['MMCODE'], rowIdx, '1');
                }
            }
        }, {
            text: '乙案',
            dataIndex: 'PURCHASE2',
            style: 'text-align:left',
            align: 'center',
            sortable: false,
            width: 50,
            renderer: function (val, meta, record) {
                var disabledString = '';
                if (record.data['SIGN_TIME'] != null && record.data['SIGN_TIME'] != '')
                    disabledString = 'disabled';
                if (val == 'Y')
                    return "<input type='radio' checked='checked' " + disabledString + " >";
                else
                    return "<input type='radio' " + disabledString + " >";
            },
            listeners: {
                click: function (grid, c, rowIdx, colIdx, e, record) {
                    if (record.data['PURCHASE2'] != 'Y')
                        updatePurtype(record.data['MMCODE'], rowIdx, '2');
                }
            }
        }, {
            sortable: false,
            flex: 1
        }],
        renderTo: Ext.getBody()
    });

    function chkTempData() {
        Ext.Ajax.request({
            url: chkTempGet,
            params: {
                p0: T1Query.getForm().findField('P0').rawValue,
                p1: T1Query.getForm().findField('P1').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'sign')
                    {
                        Ext.Msg.alert('訊息', '此月份資料已轉正式,只能查詢');
                        T1Load();
                    }
                    else if (data.msg == 'dup')
                    {
                        Ext.MessageBox.confirm('訊息', '此月份資料已存在,是否要覆蓋?', function (btn, text) {
                            if (btn === 'yes') {
                                replaceTempData();
                            }
                            else
                                T1Load();
                        });
                    }
                    else if (data.msg == 'query')
                        T1Load();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function replaceTempData() {
        Ext.Ajax.request({
            url: rpTempGet,
            params: {
                p0: T1Query.getForm().findField('P0').rawValue,
                p1: T1Query.getForm().findField('P1').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Load();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function updatePurtype(par_mmcode, rowIdx, par_purtype) {
        // 在送request之前就更新畫面
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        if (par_purtype == '1') {
            T1Store.data.items[rowIdx].data.PURCHASE1 = 'Y';
            T1Store.data.items[rowIdx].data.PURCHASE2 = '';
        }
        else if (par_purtype == '2') {
            T1Store.data.items[rowIdx].data.PURCHASE1 = '';
            T1Store.data.items[rowIdx].data.PURCHASE2 = 'Y';
        }
        T1Grid.setStore(T1Store);
        

        Ext.Ajax.request({
            url: purtypeSet,
            params: {
                mmcode: par_mmcode,
                purtype: par_purtype
            },
            method: reqVal_p,
            success: function (response) {
                //var data = Ext.decode(response.responseText);
                //if (par_purtype == '1')
                //{
                //    T1Store.data.items[rowIdx].data.PURCHASE1 = 'Y';
                //    T1Store.data.items[rowIdx].data.PURCHASE2 = '';
                //}
                //else if (par_purtype == '2') {
                //    T1Store.data.items[rowIdx].data.PURCHASE1 = '';
                //    T1Store.data.items[rowIdx].data.PURCHASE2 = 'Y';
                //}
                    
                //T1Grid.setStore(T1Store);
                myMask.hide();
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    function updateMastPurtype() {
        Ext.Ajax.request({
            url: mastPurtypeSet,
            method: reqVal_p,
            params: {
                p0: T1Query.getForm().findField('P0').rawValue
            },
            success: function (response) {
                Ext.Msg.alert('訊息', '資料轉正式完成', function () { T1Load(); });
            },
            failure: function (response, options) {

            }
        });
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
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });
    T1Query.getForm().findField('P1').focus();
    T1Query.getForm().findField('P0').setValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
});
