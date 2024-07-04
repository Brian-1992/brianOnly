﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var reportUrl = '/Report/A/AB0070.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AB0070/GetWH_NOComboOne';
    var DiffClsComboGet = '../../../api/AB0070/GetDiffClsComboOne';

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    // 異動類別
    var st_DiffCls = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var T2QueryMMCodeStart = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P5',
        name: 'P5',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 314,
        labelWidth: 74,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0070/GetMMCODEComboOne', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    var T2QueryMMCodeEnd = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P6',
        name: 'P6',
        fieldLabel: '至',
        labelSeparator: '',
        labelWidth: 16,
        width: 256,
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0070/GetMMCODEComboOne', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    function setComboData() {

        Ext.Ajax.request({
            url: WH_NOComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P0 = T1Query.getForm().findField('P0');

                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            wh_noStore.add({ WH_NO: tb_wh_no[i].VALUE, WH_NAME: tb_wh_no[i].COMBITEM });
                            combo_P0.setValue('PH1S'); //預設為PH1S_藥庫
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: DiffClsComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_DiffCls = data.etts;
                    var combo_P1 = T1Query.getForm().findField('P1');

                    if (tb_DiffCls.length > 0) {
                        st_DiffCls.add({ VALUE: '', COMBITEM: '全部' });
                        for (var i = 0; i < tb_DiffCls.length; i++) {
                            st_DiffCls.add({ VALUE: tb_DiffCls[i].VALUE, COMBITEM: tb_DiffCls[i].COMBITEM });
                            combo_P1.setValue('MR'); //預設為藥品申請
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'combo',
                    store: wh_noStore,
                    fieldLabel: '庫別',
                    name: 'P0',
                    id: 'P0',
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'combo',
                    store: st_DiffCls,
                    fieldLabel: '異動類別',
                    name: 'P1',
                    id: 'P1',
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 65,
                    width: 190,
                    name: 'P2',
                    id: 'P2',
                    items: [
                        {
                            boxLabel: '出入庫',
                            width: 70,
                            name: 'P2',
                            inputValue: 'IO',
                            checked: true
                        },
                        {
                            boxLabel: '出庫',
                            width: 60,
                            name: 'P2',
                            inputValue: 'O'
                        },
                        {
                            boxLabel: '入庫',
                            width: 60,
                            name: 'P2',
                            inputValue: 'I'
                        }
                    ],
                    padding: '0 4 0 15'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 45px;',
                    handler: function () {
                        if (Ext.getCmp('P0').validate() && Ext.getCmp('P1').validate()
                            && Ext.getCmp('P3').validate() && Ext.getCmp('P4').validate()) {
                            showReport();
                        }
                        else {
                            Ext.Msg.alert('訊息', '庫別、異動類別、異動日期起迄為必填條件');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '異動日期',
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 74,
                    width: 164,
                    value: getDefaultValue(false),
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    id: 'P4',
                    labelWidth: 20,
                    width: 110,
                    value: getDefaultValue(true),
                    fieldCls: 'required',
                    allowBlank: false,
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                T2QueryMMCodeStart,
                T2QueryMMCodeEnd
            ]
        }]
    });

    function getDefaultValue(isEndDate) {
        tmp_Date = new Date();
        if (!isEndDate) {
            tmp_Date.setDate(tmp_Date.getDate() - 3);//3天前的日期
            var y = tmp_Date.getFullYear() - 1911;
            var m = tmp_Date.getMonth() + 1;
            var d = tmp_Date.getDate();
            m = m > 9 ? m.toString() : "0" + m.toString();
            d = d > 9 ? d.toString() : "0" + d.toString();
            tmp_Date = y + m + d;
        }
        return tmp_Date;
    }      

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
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
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p1_Name: T1Query.getForm().findField('P1').getRawValue(),
                p2: T1Query.getForm().findField('P2').getValue().P2,
                p3: T1Query.getForm().findField('P3').getRawValue(),
                p4: T1Query.getForm().findField('P4').getRawValue(),
                p5: T1Query.getForm().findField('P5').getValue(),
                p6: T1Query.getForm().findField('P6').getValue(),
            };

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p1_Name=' + np.p1_Name + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        },
        ]
    });
});